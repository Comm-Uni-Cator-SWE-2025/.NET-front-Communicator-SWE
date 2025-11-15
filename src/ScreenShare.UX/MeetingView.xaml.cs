// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace ScreenShare.UX;
/// <summary>
/// Interaction logic for MeetingView.xaml
/// </summary>
public partial class MeetingView : UserControl
{

    public ObservableCollection<ParticipantData> Participants { get; set; }
    private int participantCounter = 1; // Counter for generating participant names

    public MeetingView()
    {

        InitializeComponent();
    }
}
