namespace GSTHD
{
    public struct DragDropContent
    {
        public bool IsAutocheck;
        public string ImageName;
        public int dk_id;
        public MarkedImageIndex isMarked;

        public DragDropContent(bool isAutocheck, string imageName, int dkid = -1, MarkedImageIndex marked = 0)
        {
            IsAutocheck = isAutocheck;
            ImageName = imageName;
            dk_id = dkid;
            isMarked = marked;
        }

        public override string ToString()
        {
            return $"{IsAutocheck}|{ImageName}|{dk_id}|{isMarked}";
        }
    }
}
