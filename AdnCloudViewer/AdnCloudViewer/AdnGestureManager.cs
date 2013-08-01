/////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2013 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
/////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace AdnCloudViewer
{
    /////////////////////////////////////////////////////////////////////////////
    // 
    //
    /////////////////////////////////////////////////////////////////////////////
    class AdnGestureManager
    {       
        private GestureRecognizer _gr;
        private AdnRenderer _renderer;
        private UIElement _window;

        private double _dragDistance;
        private PointerMode _pointerMode;
        private Point _previousPointerPos;

        private Dictionary<uint, PointerPoint> 
            _pointers;

        private ValueAccumulator 
            _accumulator;

        enum PointerMode
        { 
            kBeginDragMode,
            kDragMode,
            kScaleMode,
            kIdleMode
        }

        public AdnGestureManager(
            UIElement window, 
            AdnRenderer renderer)
        {
            _window = window;

            _renderer = renderer;

            _pointerMode = PointerMode.kIdleMode;

            _accumulator = new ValueAccumulator(
                -30.0 * 1000.0 / 5.0, 
                -100.0 * 1000.0 / 5.0, 
                -5.0 * 1000.0 / 5.0);

            _pointers = new Dictionary<uint, PointerPoint>();

            window.PointerMoved += OnPointerMoved;
            window.PointerPressed += OnPointerPressed;         
            window.PointerReleased += OnPointerReleased;
            window.PointerWheelChanged += OnPointerWheelChanged;

            _gr = new GestureRecognizer();

            _gr.GestureSettings =
              GestureSettings.Tap |
              GestureSettings.Drag |          
              GestureSettings.DoubleTap |
              GestureSettings.ManipulationScale;

            _gr.Tapped += OnTapped;
            _gr.ManipulationStarted += OnManipulationStarted;
            _gr.ManipulationUpdated += OnManipulationUpdated;
            _gr.ManipulationCompleted += OnManipulationCompleted;
        }

        void OnPointerWheelChanged(
            object sender, 
            PointerRoutedEventArgs e)
        {
            var pointer = 
                e.GetCurrentPoint(_window);

            float dZ =
                pointer.Properties.MouseWheelDelta * 0.01f;

            _renderer.AddZoom(dZ);

            Point pos = ToDeviceCoordinate(
                pointer.RawPosition);

            _renderer.CheckPreSelection(
                 (float)pos.X,
                 (float)pos.Y);
        }

        void OnPointerPressed(
            object sender,
            PointerRoutedEventArgs e)
        {
            _gr.ProcessDownEvent(
                e.GetCurrentPoint(_window));

            PointerPoint pointer =
                e.GetCurrentPoint(_window);

            switch (_pointerMode)
            { 
                // First pointer being pressed
                case PointerMode.kIdleMode:

                    // No drag so far
                    _dragDistance = 0.0;

                    _pointers.Add(
                        pointer.PointerId, 
                        pointer);

                    // Enter drag mode
                    _pointerMode = PointerMode.kBeginDragMode;
                    break;

                // Second finger being pressed
                case PointerMode.kBeginDragMode:
                case PointerMode.kDragMode:

                    if (_pointers.Count < 2)
                    {
                        _pointers.Add(
                            pointer.PointerId,
                            pointer);

                        _pointerMode = PointerMode.kScaleMode;
                    }
                    break;

                case PointerMode.kScaleMode:
                default:
                    break;
            }
        }

        void OnPointerMoved(
            object sender, 
            PointerRoutedEventArgs e)
        {
            _gr.ProcessMoveEvents(
                e.GetIntermediatePoints(_window));

            PointerPoint pointer =
                e.GetCurrentPoint(_window);

            if (_pointers.ContainsKey(pointer.PointerId))
                _pointers[pointer.PointerId] = pointer;

            switch (_pointerMode)
            {
                case PointerMode.kBeginDragMode:

                    _previousPointerPos =
                        pointer.Position;

                    _pointerMode = PointerMode.kDragMode;
                    break;

                case PointerMode.kDragMode:

                    double xOffset =
                        _previousPointerPos.X - 
                        pointer.Position.X;

                    double yOffset =
                        _previousPointerPos.Y - 
                        pointer.Position.Y;

                    _dragDistance += GetDistance(
                       _previousPointerPos,
                       pointer.Position);

                    _previousPointerPos = 
                        pointer.Position;

                    _renderer.Rotate(
                        (float)xOffset,
                        (float)yOffset);

                    break;

                case PointerMode.kScaleMode:

                    var pointers =
                        _pointers.Values.ToList();

                    double dist = GetDistance(
                        ToDeviceCoordinate(pointers[0].RawPosition),
                        ToDeviceCoordinate(pointers[1].RawPosition));

                    double accZoom = 
                        _accumulator.Accumulate(dist);

                    _renderer.SetZoom((float)accZoom * 5.0f / 1000.0f);

                    break;

                case PointerMode.kIdleMode:

                   Point pos = ToDeviceCoordinate(
                        pointer.RawPosition);
     
                    _renderer.CheckPreSelection(
                         (float)pos.X,
                         (float)pos.Y);

                    break;

                default:
                    break;
            }
        }

        void OnPointerReleased(
           object sender,
           PointerRoutedEventArgs e)
        {
            PointerPoint pointer =
                e.GetCurrentPoint(_window);

            if (!_pointers.ContainsKey(pointer.PointerId))
                return;

            _gr.ProcessUpEvent(
                e.GetCurrentPoint(_window));

            _pointers.Remove(pointer.PointerId);

            switch (_pointerMode)
            {
                case PointerMode.kBeginDragMode:
                case PointerMode.kDragMode:

                    if (_dragDistance < 5)
                    {
                        Point pos = ToDeviceCoordinate(
                            pointer.RawPosition);
     
                        _renderer.CheckSelection(
                            (float)pos.X,
                            (float)pos.Y);
                    }

                    _pointerMode = PointerMode.kIdleMode;

                    break;

                case PointerMode.kScaleMode:

                    _pointerMode = PointerMode.kBeginDragMode;
                    _accumulator.SetCurrentValue(0);
                    break;

                default:
                    break;
            }
        }

        void OnTapped(
            GestureRecognizer sender,
            TappedEventArgs args)
        {
            if (args.TapCount > 1)
            {
                Point pos = ToDeviceCoordinate(
                    args.Position);

                 _renderer.CheckDisplaySelection(
                    (float)pos.X,
                    (float)pos.Y);
            }
        }

        void OnManipulationCompleted(
            GestureRecognizer sender, 
            ManipulationCompletedEventArgs args)
        {
            
        }

        void OnManipulationUpdated(
            GestureRecognizer sender, 
            ManipulationUpdatedEventArgs args)
        {
          
        }

        void OnManipulationStarted(
            GestureRecognizer sender, 
            ManipulationStartedEventArgs args)
        {
            
        }

        static Point ToDeviceCoordinate(Point point)
        {
            Point result = new Point(
                (point.X * DisplayProperties.LogicalDpi) / 96.0,
                (point.Y * DisplayProperties.LogicalDpi) / 96.0);

            return result;
        }

        static double GetDistance(Point p1, Point p2)
        {
            double xDist =
                p1.X -
                p2.X;

            double yDist =
                p1.Y -
                p2.Y;

            return Math.Sqrt(
                xDist * xDist +
                yDist * yDist);
        }
    }

    class ValueAccumulator
    {
        double _currentValue;
        double _accumulator;

        double _minAccValue;
        double _maxAccValue;

        public ValueAccumulator(
            double initialAcc, 
            double minAccValue, 
            double maxAccValue)
        {
            _minAccValue = minAccValue;
            _maxAccValue = maxAccValue;

            _currentValue = 0.0;
            _accumulator = initialAcc;
        }

        public double Accumulate(double value)
        {
            double delta = 0;

            if (_currentValue == 0.0)
                _currentValue = value;
            else
            {
                delta = value - _currentValue;
                _currentValue = value;
            }

            _accumulator += delta;

            if (_accumulator < _minAccValue)
                _accumulator = _minAccValue;

            if (_accumulator > _maxAccValue)
                _accumulator = _maxAccValue;

            return _accumulator;
        }

        public void SetCurrentValue(double value)
        {
            _currentValue = value;
        }
    }
}
