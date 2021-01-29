using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace WebDemo2.Service
{
    public class CaptchaHelper
    {
        #region 参数
        //裁剪的小图大小
        private const int _shearSize = 40;
        //原始图片所在路径 300*200
        //原始图片数量
        private const int _imgNum = 8;
        //原始图片宽px
        private readonly int _imgWidth = 300;
        //原始图片高px
        private readonly int _imgHeight = 300;
        //裁剪位置X轴最小位置
        private readonly int _minRangeX = 30;
        //裁剪位置X轴最大位置
        private readonly int _maxRangeX = 240;
        //裁剪位置Y轴最小位置
        private readonly int _minRangeY = 30;
        //裁剪位置Y轴最大位置
        private readonly int _maxRangeY = 200;
        //裁剪X轴大小 裁剪成20张上10张下10张
        private readonly int _cutX = 30;
        //裁剪Y轴大小 裁剪成20张上10张下10张
        private readonly int _cutY = 150;
        //小图相对原图左上角的x坐标  x坐标保存到session 用于校验
        public int _PositionX { get; set; }

        //图片规格列表 默认300*200（"300*300", "300*200", "200*100"）
        //允许误差 单位像素
        public const int _deviationPx = 2;
        //最大错误次数
        public const int _MaxErrorNum = 4;
        #endregion

        /// <summary>
        /// 获取验证码
        /// </summary>
        /// <returns></returns>
        public object GetVerificationCode(string path)
        {
            //小图相对原图左上角的y坐标  y坐标返回到前端
            int _positionY;
            var rd = new Random();

            _PositionX = rd.Next(_minRangeX, _maxRangeX);

            _positionY = rd.Next(_minRangeY, _maxRangeY);

            int[] numbers = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19 };

            int[] array = numbers.OrderBy(x => Guid.NewGuid()).ToArray();

            path = Path.Combine(path, $"{new Random().Next(1, _imgNum)}.jpg");

            Bitmap bmp = new Bitmap(path);

            string ls_small = "data:image/jpg;base64," + ImgToBase64String(CutImage(bmp, _shearSize, _shearSize, _PositionX, _positionY));

            Bitmap lb_normal = GetNewBitMap(bmp, _shearSize, _shearSize, _PositionX, _positionY);

            string ls_confusion = "data:image/jpg;base64," + ImgToBase64String(ConfusionImage(array, lb_normal));

            /* errcode: 状态值 成功为0
             * y:裁剪图片y轴位置
             * small：小图字符串
             * normal：剪切小图后的原图并按无序数组重新排列后的图
             * array：无序数组
             * imgx：原图宽
             * imgy：原图高
             */
            return new
            {
                errcode = 0,
                y = _positionY,
                array = string.Join(",", array),
                imgx = _imgWidth,
                imgy = _imgHeight,
                small = ls_small,
                normal = ls_confusion
            };
        }



        /// <summary>
        /// 获取裁剪的小图
        /// </summary>
        /// <param name="sFromBmp">原图</param>
        /// <param name="cutWidth">剪切宽度</param>
        /// <param name="cutHeight">剪切高度</param>
        /// <param name="x">X轴剪切位置</param>
        /// <param name="y">Y轴剪切位置</param>
        private Bitmap CutImage(Bitmap sFromBmp, int cutWidth, int cutHeight, int x, int y)
        {
            //载入底图   
            Image fromImage = sFromBmp;

            //先初始化一个位图对象，来存储截取后的图像
            Bitmap bmpDest = new Bitmap(cutWidth, cutHeight, System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            //这个矩形定义了，你将要在被截取的图像上要截取的图像区域的左顶点位置和截取的大小
            Rectangle rectSource = new Rectangle(x, y, cutWidth, cutHeight);

            //这个矩形定义了，你将要把 截取的图像区域 绘制到初始化的位图的位置和大小
            //我的定义，说明，我将把截取的区域，从位图左顶点开始绘制，绘制截取的区域原来大小
            Rectangle rectDest = new Rectangle(0, 0, cutWidth, cutHeight);

            //第一个参数就是加载你要截取的图像对象，第二个和第三个参数及如上所说定义截取和绘制图像过程中的相关属性，第四个属性定义了属性值所使用的度量单位
            Graphics g = Graphics.FromImage(bmpDest);
            g.DrawImage(fromImage, rectDest, rectSource, GraphicsUnit.Pixel);
            g.Dispose();
            return bmpDest;
        }

        /// <summary>
        /// 获取混淆拼接的图片
        /// </summary>
        /// <param name="a">无序数组</param>
        /// <param name="bmp">剪切小图后的原图</param>
        private Bitmap ConfusionImage(int[] a, Bitmap cutbmp)
        {
            Bitmap[] bmp = new Bitmap[20];
            for (int i = 0; i < 20; i++)
            {
                int x, y;
                x = a[i] > 9 ? (a[i] - 10) * _cutX : a[i] * _cutX;
                y = a[i] > 9 ? _cutY : 0;
                bmp[i] = CutImage(cutbmp, _cutX, _cutY, x, y);
            }
            Bitmap Img = new Bitmap(_imgWidth, _imgHeight);      //创建一张空白图片
            Graphics g = Graphics.FromImage(Img);   //从空白图片创建一个Graphics
            for (int i = 0; i < 20; i++)
            {
                //把图片指定坐标位置并画到空白图片上面
                g.DrawImage(bmp[i], new Point(i > 9 ? (i - 10) * _cutX : i * _cutX, i > 9 ? _cutY : 0));
            }
            g.Dispose();
            return Img;
        }

        /// <summary>
        /// 获取裁剪小图后的原图
        /// </summary>
        /// <param name="sFromBmp">原图</param>
        /// <param name="cutWidth">剪切宽度</param>
        /// <param name="cutHeight">剪切高度</param>
        /// <param name="spaceX">X轴剪切位置</param>
        /// <param name="spaceY">Y轴剪切位置</param>
        private Bitmap GetNewBitMap(Bitmap sFromBmp, int cutWidth, int cutHeight, int spaceX, int spaceY)
        {
            // 加载原图片 
            Bitmap oldBmp = sFromBmp;

            #region index 1,4,8等直接绑定到画板抛出异常，转换为24
            var bitmap24 = new Bitmap(oldBmp.Width, oldBmp.Height, PixelFormat.Format24bppRgb);
            Graphics g = Graphics.FromImage(bitmap24);
            g.DrawImageUnscaled(oldBmp, 0, 0);

            oldBmp = bitmap24;
            #endregion

            // 绑定画板 
            Graphics grap = Graphics.FromImage(oldBmp);

            // 加载水印图片 
            Bitmap bt = new Bitmap(cutWidth, cutHeight);
            Graphics g1 = Graphics.FromImage(bt);  //创建b1的Graphics
            g1.FillRectangle(Brushes.Black, new Rectangle(0, 0, cutWidth, cutHeight));   //把b1涂成红色
            bt = PTransparentAdjust(bt, 120);
            // 添加水印 
            grap.DrawImage(bt, spaceX, spaceY, cutWidth, cutHeight);
            g.Dispose();
            grap.Dispose();
            g1.Dispose();
            return oldBmp;
        }

        /// <summary>
        /// 获取半透明图像
        /// </summary>
        /// <param name="bmp">Bitmap对象</param>
        /// <param name="alpha">alpha分量。有效值为从 0 到 255。</param>
        private Bitmap PTransparentAdjust(Bitmap bmp, int alpha)
        {
            for (int i = 0; i < bmp.Width; i++)
            {
                for (int j = 0; j < bmp.Height; j++)
                {
                    Color bmpcolor = bmp.GetPixel(i, j);
                    _ = bmpcolor.A;
                    byte R = bmpcolor.R;
                    byte G = bmpcolor.G;
                    byte B = bmpcolor.B;
                    bmpcolor = Color.FromArgb(alpha, R, G, B);
                    bmp.SetPixel(i, j, bmpcolor);
                }
            }
            return bmp;
        }

        //Bitmap转为base64编码的文本
        private string ImgToBase64String(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
            byte[] arr = new byte[ms.Length];
            ms.Position = 0;
            ms.Read(arr, 0, (int)ms.Length);
            ms.Close();
            return Convert.ToBase64String(arr);
        }
    }
}
