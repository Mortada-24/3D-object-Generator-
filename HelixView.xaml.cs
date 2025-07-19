using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace _3dObjectGenerator
{
    /// <summary>
    /// Interaction logic for HelixView.xaml
    /// </summary>
    public partial class HelixView : UserControl
    {
        public HelixView()
        {
            InitializeComponent();
        }
        public void LoadModel(MeshGeometry3D mesh)
        {
            // Compute normals for lighting (required!)
            mesh.Normals = MeshGeometryHelper.CalculateNormals(mesh);

            // Material with specular highlights
            var materialGroup = new MaterialGroup();
            materialGroup.Children.Add(new DiffuseMaterial(new SolidColorBrush(Colors.LightGray)));
            materialGroup.Children.Add(new SpecularMaterial(new SolidColorBrush(Colors.White), 100)); // shiny

            var model = new GeometryModel3D(mesh, materialGroup);
            model.BackMaterial = materialGroup; // render both sides

            var visual = new ModelVisual3D { Content = model };

            viewport.Children.Clear();
            viewport.Children.Add(new DefaultLights()); // includes ambient + directional
            viewport.Children.Add(visual);
        }
    }
}
