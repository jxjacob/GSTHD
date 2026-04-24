using System;
using System.Collections.Generic;
using AsyncImageLoader;
using Avalonia;
using Avalonia.Media;
using Math = System.Math;
using Color = Avalonia.Media.Color;
using Point = Avalonia.Point;
using Size = Avalonia.Size;

namespace GSTHD
{
    public enum MarkedImageIndex
    {
        none,
        mark_check,
        mark_x,
        mark_question,
        mark_star,
        need_override
    }

    public class OrganicImage : AdvancedImage
    {
        public bool isFaded = false;

        public MarkedImageIndex isMarked = 0;
        private readonly int MarkedMax = Enum.GetNames<MarkedImageIndex>().Length;
        public AdvancedImage markedImage;
        public bool isMarkable = true;

        public Color BackColor;
        public int TrueWidth;
        public int TrueHeight;
        public PictureBoxSizeMode SizeMode { get; set; } = PictureBoxSizeMode.Zoom;
        
        public OrganicImage(Uri? baseUri) : base(baseUri)
        {
            markedImage = new AdvancedImage((Uri?)null);
            ClipToBounds = false;
        }

        public OrganicImage(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            
        }
        

        public void IncrementMarked(List<Settings.MarkModeOption> enabledOptions)
        {
            if (isMarkable) {
                int tempMark = (int)isMarked;
                int runningMark = tempMark;
                while (true)
                {
                    runningMark = (runningMark + 1) % MarkedMax;
                    if (runningMark == 0) { isMarked = MarkedImageIndex.none; break; }
                    if (runningMark == tempMark) break;
                    if (enabledOptions.Contains((Settings.MarkModeOption)runningMark)) { isMarked = (MarkedImageIndex)runningMark; break; }
                }

            }
        }

        public override void Render(DrawingContext drawingContext)
        {
            var source = CurrentImage;

            if (source != null && Bounds is { Width: > 0, Height: > 0 }) {

                var sizeDims = GetSizeDims();
                
                if (isMarked > 0) markedImage.Source = $@"Resources/{isMarked}.png";
                
                var sourceRect = new Rect(sizeDims[4], sizeDims[5], sizeDims[6], sizeDims[7]);
                var destRect = new Rect(sizeDims[0], sizeDims[1], sizeDims[2], sizeDims[3]);
                
                drawingContext.DrawRectangle(new SolidColorBrush(BackColor), null, new Rect(new Point(0,0), new Size(TrueWidth,TrueHeight)));
                
                // TODO: make it so the Mark isn't also semitransparent
                drawingContext.PushOpacity(isFaded ? 0.5 : 1);
                drawingContext.DrawImage(source, sourceRect, destRect);

                
                if (isMarked > 0)
                {
                    if (8*8 < (TrueWidth * TrueHeight) / 2)
                    {
                        drawingContext.DrawImage(Layout.Insufferable[isMarked].CurrentImage, new Rect(TrueWidth - 8, 0, 8, 8));
                    } else if (6*6 < (TrueWidth * TrueHeight) / 2)
                    {
                        drawingContext.DrawImage(Layout.Insufferable[isMarked].CurrentImage, new Rect(TrueWidth - 6, 0, 6, 6));
                    }
                }
                
            }
            else {
                base.Render(drawingContext);
            }
        }

        public int[] GetSizeDims()
        {
            // newX newY newWidth newHeight
            // imgX imgY imgWidth imgHeight

            int newX, newY, newWidth, newHeight;
            switch (SizeMode){
                case PictureBoxSizeMode.Zoom:
                    float ratio = Math.Max((float)CurrentImage.Size.Width / TrueWidth, (float)CurrentImage.Size.Height / TrueHeight);
                    newWidth = (int)(CurrentImage.Size.Width / ratio);
                    newHeight = (int)(CurrentImage.Size.Height / ratio);
                    newY = (TrueHeight - newHeight) / 2;
                    newX = (TrueWidth - newWidth) / 2;
                    return [newX, newY, newWidth, newHeight, 0, 0, (int)Bounds.Width, (int)Bounds.Height];
                case PictureBoxSizeMode.CenterImage:
                    newX = (int)(TrueWidth-CurrentImage.Size.Width) / 2;
                    newY = (int)(TrueHeight-CurrentImage.Size.Height) / 2;
                    return [newX, newY, (int)CurrentImage.Size.Width, (int)CurrentImage.Size.Height, 0, 0, (int)CurrentImage.Size.Width, (int)CurrentImage.Size.Height];
                case PictureBoxSizeMode.Normal:
                    return [0, 0, TrueWidth, TrueHeight, 0, 0, TrueWidth, TrueHeight];
                case PictureBoxSizeMode.StretchImage:
                    return [0, 0, TrueWidth, TrueHeight, 0, 0, (int)CurrentImage.Size.Width, (int)CurrentImage.Size.Height];
                case PictureBoxSizeMode.AutoSize:
                default:
                    var usedWidth = (TrueWidth < CurrentImage.Size.Width) ? CurrentImage.Size.Width : TrueWidth;
                    var usedHeight = (TrueHeight < CurrentImage.Size.Height) ? CurrentImage.Size.Height : TrueHeight;
                    return [0, 0, (int)Math.Min(usedWidth, CurrentImage.Size.Width), (int)Math.Min(usedHeight, CurrentImage.Size.Height), 0, 0, (int)Math.Min(usedWidth, CurrentImage.Size.Width), (int)Math.Min(usedHeight, CurrentImage.Size.Height)];
            }
        }
    }
}
