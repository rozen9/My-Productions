using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

// 2016, 06/16, Kohei Yoshida

namespace ImageHistogram
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.Text = "Histogram of Image";
            this.Width = 1000;
            this.Height = 700;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            var img = new ImagingSolution.Imaging.ImageData(""); // 参照画像の取得
            int iw = img.Width; // 画像の幅を取得
            int ih = img.Height; // 画像の高さを取得
            int allData = 0; // 画像すべての輝度データ
            int p = 60;// 二値化する割合
            int ag;

            int[] hist = new int[256]; // 輝度を格納するための配列
            for (int i = 0; i < hist.Length; i++) hist[i] = 0; // 配列の初期化

            // 元画像の表示
            e.Graphics.DrawImage(img.ToBitmap(), 700, 320, img.Width / 2, img.Height / 2);

            /*画像1ピクセルごとに輝度の値が取得されたときに対応した配列の中身を増やしていく */
            for (int y = 0; y < ih; y++)
            {
                for (int x = 0; x < iw; x++)
                {
                    hist[img[y, x]] += 1;
                }
            }

            // 画素の全データを取得
            for (int k = 0; k < hist.Length; k++)
            {
                allData += hist[k];
            }

            // 二値化する割合に相当する画素
            ag = allData * p / 100;

            // 画素データの初期化
            allData = 0;

            // 閾値の決定
            for (int l = 0; l < hist.Length; l++)
            {
                allData += hist[l];
                if (allData > ag)
                {
                    ag = l;
                    break;
                }
            }

            /* 二値化処理 */
            for (int y2 = 0; y2 < ih; y2++)
            {
                for (int x2 = 0; x2 < iw; x2++)
                {
                    if (img[y2, x2] < ag)
                    {
                        img[y2, x2] = 0;
                    }
                    else if (img[y2, x2] >= ag)
                    {
                        img[y2, x2] = 255;
                    }
                }
            }

            Pen blue = new Pen(Color.Blue, 1);
            Pen black = new Pen(Color.Black, 1);

            /* histの配列の値を表示(実際の大きさよりも縮小) */
            for (int j = 0; j < hist.Length; j++)
            {
                e.Graphics.DrawLine(blue, 10, 10 + j, (hist[j] / 10) + 10, 10 + j);
            }

            /* 軸 */
            e.Graphics.DrawLine(black, 10, 5, 500, 5);
            e.Graphics.DrawLine(black, 10, 5, 10, 300);

            Bitmap im = img.ToBitmap();

            // 二値化した画像の出力
            e.Graphics.DrawImage(im, 100, 320, img.Width / 2, img.Height / 2);




            ///////////////////////////////////// 4 近傍連結 ////////////////////////////////////

            int[] b = new int[im.Width * im.Height];
            List<int> lst = new List<int>();
            int siz = 3;
            int lbnum = 0;
            int idx = 0;
            int x1, y1;
            Color cl;

            for (int y = 0; y < im.Height; y++)
            {
                for (int x = 0; x < im.Width; x++)
                {
                    // ラベルが貼られていない時
                    if (b[y * im.Width + x] == 0)
                    {
                        cl = im.GetPixel(x, y);

                        // 次のラベルに移行
                        lbnum++;
                        b[y * im.Width + x] = lbnum;

                        // 未調査座標を追加
                        lst.Add(y * im.Width + x);
                        do
                        {
                            idx = 0;
                            x1 = lst[0] % im.Width;
                            y1 = lst[0] / im.Width;

                            for (int i = (y1 - (siz / 2)); i <= (y1 + (siz / 2)); i++)
                            {
                                for (int j = (x1 - (siz / 2)); j <= (x1 + (siz / 2)); j++)
                                {
                                    if ((i >= 0 && i < im.Height) && (j >= 0 && j < im.Width))
                                    {
                                        // 上下左右の時
                                        if (idx % 2 == 1)
                                        {
                                            // すでにラベルが貼られているか
                                            if (b[i * im.Width + j] == 0)
                                            {
                                                // 色が同じ時
                                                if (im.GetPixel(j, i) == cl)
                                                {
                                                    // ラベルを貼る
                                                    b[i * im.Width + j] = lbnum;

                                                    // ラベルを貼った座標を未調査座標に追加する
                                                    lst.Add(i * im.Width + j);
                                                }
                                            }
                                        }
                                    }
                                    idx++;
                                }
                            }
                            lst.RemoveAt(0);
                        } while (lst.Count > 0);
                        lst.Clear();
                    }
                }
            }

            /*
            // ラベルが貼られている連結成分を数える
            for (int j = 0; j < im.Height; j++)
            {
                for (int i = 0; i < im.Width; i++)
                {
                    if (b[j * im.Width + i] == 1)
                    {
                        counter++;
                    }
                }
            }

            */
            // Console.WriteLine("Chain:" + counter);
            Console.WriteLine(lbnum);

            // 連結成分の個数をformに表示
            Label lb = new Label();
            lb.Parent = this;
            lb.Text = "Chain:" + lbnum;
            lb.Left = 600;
            lb.Top = 50;
            lb.Width = 300;
            lb.Height = 200;
            lb.Font = new Font(lb.Font.FontFamily, 36);

        }
    }
}
