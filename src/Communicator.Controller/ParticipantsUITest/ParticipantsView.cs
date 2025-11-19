using Communicator.Analytics;
using Communicator.Meeting;

namespace ParticipantsUITest;

public class ParticipantsView : Form
{
    public ParticipantsView()
    {
        Text = "Meeting Participants";
        Width = 500;
        Height = 400;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(250, 252, 255);

        var title = new Label {
            Text = "Meeting Participants",
            Font = new Font("Segoe UI Semibold", 16),
            ForeColor = Color.FromArgb(0, 120, 215),
            Dock = DockStyle.Top,
            TextAlign = ContentAlignment.MiddleCenter,
            Height = 50,
        };
        Controls.Add(title);

        var analytics = new UserAnalytics();
        analytics.FetchUsersFromCloud();

        var panel = new FlowLayoutPanel {
            Dock = DockStyle.Fill,
            AutoScroll = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(40),
            AutoSize = false
        };


        Controls.Add(panel);

        foreach (UserProfile user in analytics.GetAllUsers())
        {
            panel.Controls.Add(new ParticipantItemControl(user));
        }

        var closeButton = new Button {
            Text = "Close",
            BackColor = Color.FromArgb(0, 120, 215),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 12),
            Height = 40,
            Dock = DockStyle.Bottom
        };
        closeButton.Click += (s, e) => Close();
        Controls.Add(closeButton);
    }
}

