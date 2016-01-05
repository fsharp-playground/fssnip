

//MainViewModel.fs
#light
//Expression Blend support: wrap to namespace, not module
namespace HelloAppViewModel

open System
open System.ComponentModel

type MainViewModel() =

    let mutable myname = ""
    
    let event = new Event<_,_>()
    interface INotifyPropertyChanged with
        [<CLIEvent>]
        member x.PropertyChanged = event.Publish
    
    member x.TriggerPropertyChanged(name)=
        event.Trigger(x, new PropertyChangedEventArgs(name))
    
    member x.MyName 
        with get() = myname
        and set t = 
                myname <- t
                x.TriggerPropertyChanged "MyName"
                x.TriggerPropertyChanged "HiLabel"

    member x.HiLabel = "Hello " + x.MyName + "!"

////---------------------------------------------------------------------------------------
////View codebehind: MainPage.xaml.cs
//
//using System.Windows.Controls;
//namespace HelloApp
//{
//	public partial class MainPage : UserControl
//	{
//		public MainPage()
//		{
//			  // Required to initialize variables
//			  InitializeComponent();
//            this.DataContext = new HelloAppViewModel.MainViewModel();
//		}
//	}
//}

////---------------------------------------------------------------------------------------
////View xaml: Mainpage.xaml
//<UserControl
//	xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
//	xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
//	xmlns:i="http://schemas.microsoft.com/expression/2010/interactivity" xmlns:ei="http://schemas.microsoft.com/expression/2010/interactions"
//	xmlns:d="http://schemas.microsoft.com/expression/blend/2008" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" mc:Ignorable="d"
//	x:Class="HelloApp.MainPage"
//	Width="640" Height="480">
//
//	<Grid x:Name="LayoutRoot" Background="White" d:DataContext="{d:DesignData /SampleData/MainViewModelSampleData.xaml}">
//		<StackPanel HorizontalAlignment="Left" VerticalAlignment="Top" Width="400">
//			<TextBlock TextWrapping="Wrap" Text="Input your name:"/>
//			<StackPanel Orientation="Horizontal">
//				<TextBox x:Name="MyNameTextbox" TextWrapping="Wrap" Text="{Binding MyName, Mode=TwoWay}" HorizontalAlignment="Left" VerticalAlignment="Top" Width="200">
//					<i:Interaction.Triggers>
//						<i:EventTrigger EventName="TextChanged">
//							<ei:ChangePropertyAction TargetName="HelloLabel" PropertyName="Visibility"/>
//						</i:EventTrigger>
//					</i:Interaction.Triggers>
//				</TextBox>
//				<Button Content="Hi!" HorizontalAlignment="Right" VerticalAlignment="Top"/>
//			</StackPanel>
//			<TextBlock x:Name="HelloLabel" TextWrapping="Wrap" Text="{Binding HiLabel}">
//				<i:Interaction.Triggers>
//					<i:EventTrigger>
//						<ei:ChangePropertyAction PropertyName="Visibility">
//							<ei:ChangePropertyAction.Value>
//								<Visibility>Collapsed</Visibility>
//							</ei:ChangePropertyAction.Value>
//						</ei:ChangePropertyAction>
//					</i:EventTrigger>
//				</i:Interaction.Triggers>
//			</TextBlock>
//		</StackPanel>
//	</Grid>
//</UserControl>

