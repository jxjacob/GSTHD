using Avalonia;
using Avalonia.Input;

namespace GSTHD
{
    public interface DraggableElement<T>
    {
        T GetState();
        void SetState(T state);

        void StartDragDrop(PointerEventArgs e);
        void SaveChanges();
        void CancelChanges();
    }

    public class DraggableElementBehaviour<T>
    {
        protected DraggableElement<T> Element;
        protected Settings Settings;

        protected bool LeftClickDown = false;
        protected bool RightClickDown = false;
        protected bool MiddleClickDown = false;
        protected bool WheelDown = false;

        protected bool CanDragDrop = false;
        protected Point DragStartPoint;

        protected T LastState;

        public DraggableElementBehaviour(DraggableElement<T> element, Settings settings)
        {
            Element = element;
            Settings = settings;

            LastState = Element.GetState();
        }

        public void Mouse_ClickUp(object? sender, PointerReleasedEventArgs e)
        {
            switch (e.InitialPressMouseButton)
            {
                case MouseButton.Left:
                    Mouse_LeftClickUp(sender, e);
                    break;
                case MouseButton.Right:
                    Mouse_RightClickUp(sender, e);
                    break;
                case MouseButton.Middle:
                    Mouse_MiddleClickUp(sender, e);
                    break;
            }
            if (!LeftClickDown && !MiddleClickDown && !RightClickDown)
                SaveChanges();

            UpdateDragDropPreparationStatus(e);
        }

        public void Mouse_LeftClickUp(object? sender, PointerReleasedEventArgs e)
        {
            LeftClickDown = false;
        }

        public void Mouse_MiddleClickUp(object? sender, PointerReleasedEventArgs e)
        {
            MiddleClickDown = false;
        }

        public void Mouse_RightClickUp(object? sender, PointerReleasedEventArgs e)
        {
            RightClickDown = false;
        }

        public void Mouse_ClickDown(object? sender, PointerPressedEventArgs e)
        {
            if (e.Properties.IsLeftButtonPressed) Mouse_LeftClickDown(sender, e);
            else if (e.Properties.IsRightButtonPressed) Mouse_RightClickDown(sender, e);
            else if (e.Properties.IsMiddleButtonPressed) Mouse_MiddleClickDown(sender, e);
            UpdateDragDropPreparationStatus(e);
        }

        public void Mouse_LeftClickDown(object? sender, PointerPressedEventArgs e)
        {
            LeftClickDown = true;
        }

        public void Mouse_MiddleClickDown(object? sender, PointerPressedEventArgs e)
        {
            MiddleClickDown = true;
        }

        public void Mouse_RightClickDown(object? sender, PointerPressedEventArgs e)
        {
            RightClickDown = true;
        }

        public virtual void Mouse_Move(object? sender, PointerEventArgs e)
        {
            if (CanDragDrop && DragOverThreshold(e))
            {
                LeftClickDown = false;
                MiddleClickDown = false;
                RightClickDown = false;
                CanDragDrop = false;
                CancelChanges();
                Element.StartDragDrop(e);
            }
        }

        public virtual void Mouse_Wheel(object? sender, PointerWheelEventArgs e)
        {
            SaveChanges();
        }

        protected bool DragOverThreshold(PointerEventArgs e)
        {
            var point = e.GetPosition(null);
            return (System.Math.Abs(DragStartPoint.X - point.X) >= Settings.MinDragThreshold
                || System.Math.Abs(DragStartPoint.Y - point.Y) >= Settings.MinDragThreshold);
        }

        public virtual void UpdateDragDropPreparationStatus(PointerEventArgs e)
        {
            CanDragDrop = ((Settings.DragButton == Settings.DragButtonOption.LeftAndRight || Settings.AutocheckDragButton == Settings.DragButtonOption.LeftAndRight) && LeftClickDown && RightClickDown)
                || ((Settings.DragButton == Settings.DragButtonOption.Left || Settings.AutocheckDragButton == Settings.DragButtonOption.Left) && LeftClickDown)
                || ((Settings.DragButton == Settings.DragButtonOption.Right || Settings.AutocheckDragButton == Settings.DragButtonOption.Right) && RightClickDown)
                || ((Settings.DragButton == Settings.DragButtonOption.Middle || Settings.AutocheckDragButton == Settings.DragButtonOption.Middle) && MiddleClickDown)
                || ((Settings.DragButton == Settings.DragButtonOption.Control || Settings.AutocheckDragButton == Settings.DragButtonOption.Control) && LeftClickDown && e.KeyModifiers == KeyModifiers.Control )
                || ((Settings.DragButton == Settings.DragButtonOption.Shift || Settings.AutocheckDragButton == Settings.DragButtonOption.Shift) && LeftClickDown && e.KeyModifiers == KeyModifiers.Shift )
                || ((Settings.DragButton == Settings.DragButtonOption.Alt || Settings.AutocheckDragButton == Settings.DragButtonOption.Alt) && LeftClickDown && e.KeyModifiers == KeyModifiers.Alt );
            if (CanDragDrop)
                DragStartPoint = e.GetPosition(null);
        }

        public void SaveChanges()
        {
            Element.SaveChanges();
            LastState = Element.GetState();
        }

        public void CancelChanges()
        {
            Element.SetState(LastState);
            Element.CancelChanges();
        }
    }
}