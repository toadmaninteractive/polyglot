using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace Polyglot.CustomControls
{
    public partial class LoadIndicator : UserControl
    {
        Storyboard animationPoint1 = null;
        Storyboard animationPoint2 = null;
        Storyboard animationPoint3 = null;
        Storyboard animationPoint4 = null;
        Storyboard animationPoint5 = null;

        public LoadIndicator()
        {
            InitializeComponent();

            animationPoint1 = (Storyboard)this.canvas.TryFindResource("MetroLoadingAnimation1");
            animationPoint2 = (Storyboard)this.canvas.TryFindResource("MetroLoadingAnimation2");
            animationPoint3 = (Storyboard)this.canvas.TryFindResource("MetroLoadingAnimation3");
            animationPoint4 = (Storyboard)this.canvas.TryFindResource("MetroLoadingAnimation4");
            animationPoint5 = (Storyboard)this.canvas.TryFindResource("MetroLoadingAnimation5");
        }

        public void BeginStoryboard()
        {
            animationPoint1.Begin();
            animationPoint2.Begin();
            animationPoint3.Begin();
            animationPoint4.Begin();
            animationPoint5.Begin();
        }

        public void StopStoryboard()
        {
            animationPoint1.Stop();
            animationPoint2.Stop();
            animationPoint3.Stop();
            animationPoint4.Stop();
            animationPoint5.Stop();
        }
    }
}
