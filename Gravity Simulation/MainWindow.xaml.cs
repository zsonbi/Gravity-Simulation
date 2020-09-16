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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Gravity_Simulation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Ellipse dot; //The dot itself
        private bool running = false; //a bool which stores if the animation is running
        private List<TextBox> inputs = new List<TextBox>(); //A list which stores the textboxes so it's easier to index to them
        private List<float> inputvalues = new List<float>(); //A list which stores the value of the inputvalues so it's easier to fill them up
        private GravityCalculator calculator;
        private List<Label> xcordLabels = new List<Label>();
        private List<Label> heightLabels = new List<Label>();
        private float realheight = 1000;    //How much meter is it in the real world
        private float realxcord = 1000;     //How much meter is it in the real world
        private float xrate; //Since the real world value not the same as the cord we need this to get the rate with the x axle
        private float yrate; //Since the real world value not the same as the cord we need this to get the rate with the y axle
        private List<Ellipse> MarkerDots = new List<Ellipse>();
        private Stopwatch timer;
        private bool paused = false;

        //Properties

        private float height { get => inputvalues[0]; set => inputvalues[0] = value; }
        private float xcord { get => inputvalues[1]; set => inputvalues[1] = value; }
        private float speed { get => inputvalues[2]; set => inputvalues[2] = value; }
        private float angle { get => inputvalues[3]; set => inputvalues[3] = value; }

        //*******************************************************************************
        public MainWindow()
        {
            InitializeComponent();

            CreateinputList();
            CreateMarkerLists();
        }

        private void CreateMarkerLists()
        {
            for (int i = 1; i < xcordmarkergrid.Children.Count; i += 2)
            {
                xcordLabels.Add(xcordmarkergrid.Children[i] as Label);
            }

            for (int i = 1; i < heightmarkergrid.Children.Count; i += 2)
            {
                heightLabels.Add(heightmarkergrid.Children[i] as Label);
            }
            heightLabels.Reverse();
        }

        //Update the pos of the dot according to the parameters
        private void Update(float xcord, float ycord)
        {
            dot.Margin = new Thickness(xcord - 5, ycord - 5, 0, 0);
        }

        //--------------------------------------------------------------------------
        //Run the dot falling animation
        private async Task Animate()
        {
            if (!paused)
                timer = new Stopwatch();
            timer.Start();
            paused = false;
            while (!calculator.Dead && !paused)
            {
                //Calculate the current cords of the dot
                calculator.CalcCords((float)timer.Elapsed.TotalSeconds);

                //If it won't go through the top
                if (calculator.height / yrate - (float)Drawfield.ActualHeight < 0)
                {
                    Update(calculator.xcord / xrate, Math.Abs(calculator.height / yrate - (float)Drawfield.ActualHeight));
                }
                //If it would go through the top
                else
                {
                    Update(calculator.xcord / xrate, (calculator.height / yrate - (float)Drawfield.ActualHeight) * -1);
                }
                await Task.Delay(TimeSpan.FromSeconds(0.01));
                MarkerDots.Add(CreateMarkerDot((float)dot.Margin.Left, (float)dot.Margin.Top));
            }
            timer.Stop();
            if (calculator.Dead)
            {
                Startbtn.Click += StartAnimation;
                Startbtn.Click -= StopAnimation;
                Startbtn.Content = "Start";
                running = false;
            }
        }

        //-------------------------------------------------------------------------------
        //Creates a marker dot
        private Ellipse CreateMarkerDot(float xcord, float ycord)
        {
            Ellipse output = new Ellipse();
            output.Stroke = Brushes.Green;
            output.Height = 5;
            output.Width = 5;
            output.Fill = Brushes.Green;
            output.Margin = new Thickness(xcord + 2.5, ycord + 2.5, 0, 0);
            Drawfield.Children.Add(output);
            return output;
        }

        //---------------------------------------------------------------------------
        //remove the MarkerDots from the canvas
        private void RemoveMarkerDots()
        {
            foreach (var item in MarkerDots)
            {
                Drawfield.Children.Remove(item);
            }
            MarkerDots.Clear();
        }

        //---------------------------------------------------------------------------------
        //Creates the dot which we will use to indicate the movement
        private void CreateDot()
        {
            dot = new Ellipse();
            dot.Stroke = Brushes.Black;
            dot.Height = 10;
            dot.Width = 10;
            dot.Fill = Brushes.Black;
            Drawfield.Children.Add(dot);
        }

        //------------------------------------------------------------------------------
        //Update the heightbox and xtbox Text when we set the dot cords by clicking
        private void UpdateDataFields()
        {
            xcordtbox.Text = (xcord).ToString();
            Heightbox.Text = ((Drawfield.ActualHeight - (height / yrate)) * yrate).ToString();
        }

        //--------------------------------------------------------------------------------
        //Create a List which contains the txtBoxes (this gets called only in the Window's constructor)
        private void CreateinputList()
        {
            inputs.Add(Heightbox);
            inputs.Add(xcordtbox);
            inputs.Add(speedtbox);
            inputs.Add(angletbox);
        }

        //-----------------------------------------------------------------------------------
        //Change the empty txtBoxes content to 0
        private void NullOutEmptyTxtBoxes()
        {
            foreach (var item in inputs)
            {
                if (item.Text == "")
                {
                    item.Text = "0";
                }
            }
        }

        //----------------------------------------------------------------------
        //Get the requried data from the txtBoxes
        private void GetParametersFromTextBoxes()
        {
            NullOutEmptyTxtBoxes();  //Give 0 to empty txtboxes so we don't need to mess with errors
            inputvalues.Clear();    //Clear the List so we don't need to place a ton of ifs
            //fill the inputvalues
            for (int i = 0; i < inputs.Count; i++)
            {
                inputvalues.Add((float)Convert.ToDouble((inputs[i].Text)));
            }
            //reverse the height because in wpf the top is the 0 and bottom is the maxvalue
            this.height = Math.Abs((this.height / yrate) - (float)Drawfield.ActualHeight);
            this.xcord = (xcord / xrate);
            //So the previous markers are gone
            RemoveMarkerDots();
        }

        //--------------------------------------------------------------------------------
        //Change the marker Labels with the input on the y axle
        private void ChangeHeightMarkerLabels()
        {
            float temp = (float)Math.Round((Convert.ToDouble(maxheighttbox.Text) / 10f), 1);
            for (int i = 0; i < heightLabels.Count; i++)
            {
                heightLabels[i].Content = temp * i;
            }

            realheight = temp * heightLabels.Count;
        }

        //--------------------------------------------------------------------------------
        //Change the marker Labels with the input on the x axle
        private void ChangexcordMarkerLabels()
        {
            float temp = (float)Math.Round((Convert.ToDouble(Maxxsizetbox.Text) / 10f), 1);
            for (int i = 0; i < xcordLabels.Count; i++)
            {
                xcordLabels[i].Content = temp * (i + 1);
            }

            realxcord = temp * xcordLabels.Count;
        }

        //******************************************************************************
        //Handlers
        //Move the dot to the clicked Pos
        private void DrawDot(object sender, MouseButtonEventArgs e)
        {
            if (running)
            {
                return;
            }
            //Get the cords of the mouse
            Point mousecords = e.GetPosition(Drawfield);
            if (mousecords.Y > Drawfield.ActualHeight || mousecords.X > Drawfield.ActualWidth)
            {
                return;
            }

            //update the fields
            height = (float)(mousecords.Y) * yrate;
            xcord = (float)(mousecords.X) * xrate;

            Update(xcord / xrate, height / yrate);

            //Update the fields
            UpdateDataFields();
        }

        //----------------------------------------------------------------------------------
        //Set the dot Pos according to the inputtxtboxes
        private void SetDot(object sender, RoutedEventArgs e)
        {
            GetParametersFromTextBoxes();
            Update(xcord, height);

            calculator = new GravityCalculator(height, xcord, speed, angle);
        }

        //---------------------------------------------------------------------------------------------------------
        //Starts the Dot falling animation
        private async void StartAnimation(object sender, RoutedEventArgs e)
        {
            //So the user can't spam this button just in case
            if (running)
            {
                return;
            }

            running = true;

            //Change the button into a stop button
            Startbtn.Click -= StartAnimation;
            Startbtn.Click += StopAnimation;
            Startbtn.Content = "Stop";

            GetParametersFromTextBoxes();
            Update(xcord, height);
            //initialize a new calculator class since we don't check for if the data changed
            calculator = new GravityCalculator((float)Convert.ToDouble(Heightbox.Text), (float)Convert.ToDouble(xcordtbox.Text), speed, angle);
            await Animate();
        }

        //---------------------------------------------------------------------------------------------
        //Stops the animation and changes the buttons back
        private void StopAnimation(object sender, RoutedEventArgs e)
        {
            calculator.MakeItDie();
            Pausebtn.Content = "Pause";
            Pausebtn.Click -= Pausebtn_Click;
            Pausebtn.Click -= Resumebtn_Click;
            Pausebtn.Click += Pausebtn_Click;
            paused = false;
            running = false;
        }

        //-------------------------------------------------------------------------------------------------------
        //this event triggers when everything got their "ActualHeight" so we dont't get a ton of bugs
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            //Adjust the rates with the real world and the window
            yrate = (float)(realheight / Drawfield.ActualHeight);
            xrate = (float)(realxcord / Drawfield.ActualWidth);
            GetParametersFromTextBoxes();
            //Create the dot
            CreateDot();
            Update(xcord, height);
        }

        //-----------------------------------------------------------------------------------
        //update the realheight and the markerlabels
        private void maxheighttbox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangeHeightMarkerLabels();
            yrate = (float)(realheight / Drawfield.ActualHeight);
            this.height = Math.Abs(((float)Convert.ToDouble(Heightbox.Text) / yrate) - (float)Drawfield.ActualHeight);
            Update(xcord, height);
        }

        //---------------------------------------------------------------------------------------------------
        //Update realxcord and the markerlabels
        private void Maxxsizetbox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            ChangexcordMarkerLabels();
            xrate = (float)(realxcord / Drawfield.ActualWidth);
            xcord = (float)Convert.ToDouble(xcordtbox.Text) / xrate;
            Update(xcord, height);
        }

        //--------------------------------------------------------------------------------
        //Pause button (Pauses the falling)
        private void Pausebtn_Click(object sender, RoutedEventArgs e)
        {
            if (!running || calculator.Dead)
            {
                return;
            }

            paused = true;

            Pausebtn.Content = "Resume";

            Pausebtn.Click -= Pausebtn_Click;
            Pausebtn.Click += Resumebtn_Click;
        }

        //-------------------------------------------------------------------------------
        //Resume button (Resumes the falling)
        private async void Resumebtn_Click(object sender, RoutedEventArgs e)
        {
            Pausebtn.Content = "Pause";

            Pausebtn.Click += Pausebtn_Click;
            Pausebtn.Click -= Resumebtn_Click;
            running = true;
            await Animate();
        }

        //-----------------------------------------------------------------------------
        //Only accept numbers and 1 comma
        private void OnlyNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
            if (e.Text == ",")
            {
                e.Handled = (sender as TextBox).Text.Contains(",");
            }
        }
    }
}