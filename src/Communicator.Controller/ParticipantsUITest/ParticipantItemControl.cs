using Communicator.Meeting;

namespace ParticipantsUITest;

public partial class ParticipantItemControl : UserControl
{
    public ParticipantItemControl(UserProfile user)
    {
        InitializeComponent();
        BackColor = Color.FromArgb(245, 247, 250);

        var pic = new PictureBox {
            Width = 45,
            Height = 45,
            SizeMode = PictureBoxSizeMode.Zoom,
            Left = 10,
            Top = 10
        };

        LoadUserImageAsync(pic, user.LogoUrl);

        var nameLabel = new Label {
            Text = user.DisplayName,
            Font = new Font("Segoe UI Semibold", 12),
            Left = 65,
            Top = 10,
            AutoSize = true
        };

        var timeLabel = new Label {
            Text = "Joined at " + user.JoinTime,
            Font = new Font("Segoe UI", 9),
            ForeColor = Color.Gray,
            Left = 65,
            Top = 40,
            AutoSize = true
        };

        Controls.Add(pic);
        Controls.Add(nameLabel);
        Controls.Add(timeLabel);

        Height = 65;
    }

    private async void LoadUserImageAsync(PictureBox pic, string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            byte[] imageBytes = await httpClient.GetByteArrayAsync(url);
            using var ms = new MemoryStream(imageBytes);
            pic.Image = Image.FromStream(ms);
        }
        catch
        {
            pic.Image = new Bitmap(45, 45);
        }
    }

    private void InitializeComponent()
    {
        this.SuspendLayout();
        this.Name = "ParticipantItemControl";
        this.Size = new Size(500, 65);
        this.ResumeLayout(false);
    }
}

