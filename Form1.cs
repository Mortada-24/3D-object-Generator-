using RestSharp;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media.Media3D;

namespace _3dObjectGenerator
{
    public partial class Form1 : Form
    {
        private HelixView helixView;

        public Form1()
        {
            InitializeComponent();
            Button btnLoadImage = new Button
            {
                Text = "Load Image",
                Dock = DockStyle.Top,
                Height = 40
            };
            btnLoadImage.Click += btnLoadImage_Click;
            this.Controls.Add(btnLoadImage);
            Initialize3DView();
        }
        private async void btnLoadImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "PNG Files|*.png;*.jpg";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    string imagePath = ofd.FileName;
                    try
                    {
                        string apiKey = "mFjsNGu84Nq9kYkekJtHvxsJ";
                        var imageWithoutBg = await RemoveBackgroundAsync(imagePath, apiKey);

                        // Save temporarily and use it to generate mesh
                        string tempPath = Path.Combine(Path.GetTempPath(), "masked.png");
                        imageWithoutBg.Save(tempPath, ImageFormat.Png);

                        var mesh = GenerateMeshFromImage(tempPath);
                        helixView.LoadModel(mesh);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                    }
                }
            }
        }
        private void Initialize3DView()
        {
            ElementHost host = new ElementHost
            {
                Dock = DockStyle.Fill
            };

            helixView = new HelixView();
            host.Child = helixView;
            this.Controls.Add(host);
        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);


        //    if (System.IO.File.Exists(OpenFileDialog))
        //    {
        //        var mesh = GenerateMeshFromImage(imagePath);
        //        helixView.LoadModel(mesh);
        //    }
        //    else
        //    {
        //        MessageBox.Show("Image file not found.");
        //    }
        //}

        private MeshGeometry3D GenerateMeshFromImage(string imagePath)
        {
            Bitmap bmp = new Bitmap(imagePath);
            int width = bmp.Width;
            int height = bmp.Height;
            var mesh = new MeshGeometry3D();
            var indexMap = new int[width, height];

            // Add positions
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color color = bmp.GetPixel(x, y);
                    double z = color.GetBrightness() * 30.0;
                    mesh.Positions.Add(new Point3D(x, y, z));
                    indexMap[x, y] = x * height + y;
                }
            }

            // Add triangle indices
            for (int x = 0; x < width - 1; x++)
            {
                for (int y = 0; y < height - 1; y++)
                {
                    int i0 = indexMap[x, y];
                    int i1 = indexMap[x + 1, y];
                    int i2 = indexMap[x, y + 1];
                    int i3 = indexMap[x + 1, y + 1];

                    mesh.TriangleIndices.Add(i0);
                    mesh.TriangleIndices.Add(i2);
                    mesh.TriangleIndices.Add(i1);

                    mesh.TriangleIndices.Add(i1);
                    mesh.TriangleIndices.Add(i2);
                    mesh.TriangleIndices.Add(i3);
                }
            }

            return mesh;
        }
        public async Task<Image> RemoveBackgroundAsync(string imagePath, string apiKey)
        {
            var options = new RestClientOptions("https://api.remove.bg/v1.0/removebg");
            var client = new RestClient(options);

            var request = new RestRequest("", Method.Post);
            request.AddHeader("X-Api-Key", apiKey);
            request.AlwaysMultipartFormData = true;
            request.AddFile("image_file", imagePath);
            request.AddParameter("size", "auto");

            var response = await client.ExecuteAsync(request);
            if (response.IsSuccessful)
            {
                using (var ms = new MemoryStream(response.RawBytes))
                {
                    return Image.FromStream(ms);
                }
            }
            else
            {
                throw new Exception("Remove.bg API error: " + response.Content);
            }
        }
    }
}
