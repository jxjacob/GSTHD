using Avalonia.Input;

namespace GSTHD
{
    public interface DraggableAutocheckElement<T> : DraggableElement<T>
    {
        void IncrementState();
        void DecrementState();
        void ResetState();
    }

    public class DraggableAutocheckElementBehaviour<T> : DraggableElementBehaviour<T>
    {
        protected new DraggableAutocheckElement<T> Element;

        public bool AutocheckDragDrop = false;

        public DraggableAutocheckElementBehaviour(DraggableAutocheckElement<T> element, Settings settings)
            : base(element, settings)
        {
            Element = element;
            Settings = settings;

            LastState = Element.GetState();
        }

        public void Mouse_Move_WithAutocheck(object? sender, PointerEventArgs e)
        {
            if (CanDragDrop && DragOverThreshold(e))
            {
                LeftClickDown = false;
                MiddleClickDown = false;
                RightClickDown = false;
                CanDragDrop = false;
                CancelChanges();
                if (AutocheckDragDrop)
                {
                    Element.IncrementState();
                    SaveChanges();
                }
                Element.StartDragDrop(e);
            }
        }

        public override void UpdateDragDropPreparationStatus(PointerEventArgs e)
        {
            if ((Settings.AutocheckDragButton == Settings.DragButtonOption.LeftAndRight && LeftClickDown && RightClickDown)
                || (Settings.AutocheckDragButton == Settings.DragButtonOption.Middle && MiddleClickDown)
                || (Settings.AutocheckDragButton == Settings.DragButtonOption.Control && LeftClickDown && e.KeyModifiers == KeyModifiers.Control)
                || (Settings.AutocheckDragButton == Settings.DragButtonOption.Shift && LeftClickDown && e.KeyModifiers == KeyModifiers.Shift)
                || (Settings.AutocheckDragButton == Settings.DragButtonOption.Alt && LeftClickDown && e.KeyModifiers == KeyModifiers.Alt))
            {
                CanDragDrop = true;
                AutocheckDragDrop = true;
            }
            else if (Settings.DragButton == Settings.DragButtonOption.LeftAndRight && LeftClickDown && RightClickDown
                || (Settings.DragButton == Settings.DragButtonOption.Control && LeftClickDown && e.KeyModifiers == KeyModifiers.Control)
                || (Settings.DragButton == Settings.DragButtonOption.Shift && LeftClickDown && e.KeyModifiers == KeyModifiers.Shift)
                || (Settings.DragButton == Settings.DragButtonOption.Alt && LeftClickDown && e.KeyModifiers == KeyModifiers.Alt))
            {
                CanDragDrop = true;
                AutocheckDragDrop = false;
            }
            else if ((Settings.AutocheckDragButton == Settings.DragButtonOption.Left && LeftClickDown)
                || (Settings.AutocheckDragButton == Settings.DragButtonOption.Right && RightClickDown))
            {
                CanDragDrop = true;
                AutocheckDragDrop = true;
            }
            else if ((Settings.DragButton == Settings.DragButtonOption.Left && LeftClickDown)
                || (Settings.DragButton == Settings.DragButtonOption.Right && RightClickDown)
                || (Settings.DragButton == Settings.DragButtonOption.Middle && MiddleClickDown))
            {
                CanDragDrop = true;
                AutocheckDragDrop = false;
            }
            else
            {
                CanDragDrop = false;
            }
            if (CanDragDrop)
                DragStartPoint = e.GetPosition(null);
        }
    }
}
