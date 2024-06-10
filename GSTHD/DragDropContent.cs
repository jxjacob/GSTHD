using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
