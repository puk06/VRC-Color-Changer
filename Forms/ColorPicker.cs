﻿using ColorChanger.Utils;

namespace ColorChanger.Forms;

internal partial class ColorPicker : Form
{
    internal Color SelectedColor { get; private set; }
    private Point _clickedPoint;

    internal ColorPicker(Color defaultColor)
    {
        InitializeComponent();

        SelectedColor = defaultColor == Color.Empty ? Color.White : defaultColor;
        UpdateSelectedColor(SelectedColor);
    }

    private void ColorPalleteBox_MouseMove(object sender, MouseEventArgs e)
    {
        if (ColorPalleteBox.Image is not Bitmap) return;
        if (e.Button != MouseButtons.Left) return;

        // Convert clicked coordinates to original image coordinates
        var originalCoords = BitmapUtils.GetOriginalCoordinates(e.Location, ((Bitmap)ColorPalleteBox.Image).Size, ColorPalleteBox.Size);
        if (!BitmapUtils.IsValidCoordinate(originalCoords, ((Bitmap)ColorPalleteBox.Image).Size)) return;

        // Get color from the original image
        UpdateSelectedColor(((Bitmap)ColorPalleteBox.Image).GetPixel(originalCoords.X, originalCoords.Y), true);

        _clickedPoint = e.Location;
        ColorPalleteBox.Invalidate();
    }

    private void ColorPaletteBox_Paint(object sender, PaintEventArgs e)
    {
        if (ColorPalleteBox.Image is not Bitmap) return;
        if (_clickedPoint == Point.Empty) return;

        Color inverseColor = Color.FromArgb(255 - SelectedColor.R, 255 - SelectedColor.G, 255 - SelectedColor.B);
        Pen pen = new Pen(inverseColor, 2);

        e.Graphics.DrawLine(pen, _clickedPoint.X - 5, _clickedPoint.Y, _clickedPoint.X + 5, _clickedPoint.Y);
        e.Graphics.DrawLine(pen, _clickedPoint.X, _clickedPoint.Y - 5, _clickedPoint.X, _clickedPoint.Y + 5);
    }

    private void SliderChanged(object sender, EventArgs e)
    {
        UpdateSelectedColor(Color.FromArgb(RedBar.Value, GreenBar.Value, BlueBar.Value), true);
    }

    private void SliderEnd(object sender, EventArgs e)
    {
        UpdateSelectedColor(Color.FromArgb(RedBar.Value, GreenBar.Value, BlueBar.Value));
    }

    private void OnColorTextChanged(object sender, EventArgs e) => UpdateColor();

    private void TextKeyDown(object sender, KeyEventArgs e)
    {
        if (CheckKeyInput(e))
        {
            UpdateColor();
            SelectNextControl((Control)sender, true, true, true, true);
        }
    }

    private void UpdateColor()
    {
        int r = MathUtils.ParseAndClamp(RedTextBox.Text);
        int g = MathUtils.ParseAndClamp(GreenTextBox.Text);
        int b = MathUtils.ParseAndClamp(BlueTextBox.Text);

        UpdateSelectedColor(Color.FromArgb(r, g, b));
    }

    private void SelectButton_Click(object sender, EventArgs e)
    {
        UpdateColor();
        Close();
    }

    private void UpdateSelectedColor(Color color, bool noMoveMode = false)
    {
        SelectedColor = color;
        previewColorBox.BackColor = color;

        // Update UI elements
        RedBar.Value = color.R;
        GreenBar.Value = color.G;
        BlueBar.Value = color.B;

        RedTextBox.Text = color.R.ToString();
        GreenTextBox.Text = color.G.ToString();
        BlueTextBox.Text = color.B.ToString();

        colorCodeTextBox.Text = ColorUtils.GetColorCodeFromColor(color);

        // Update color palette
        if (ColorPalleteBox.Image is not Bitmap) return;
        if (noMoveMode) return;
        Point closestPoint = ColorUtils.GetClosestColorPoint(color, (Bitmap)ColorPalleteBox.Image);
        _clickedPoint = new Point((int)(closestPoint.X * (float)ColorPalleteBox.Width / ((Bitmap)ColorPalleteBox.Image).Width), (int)(closestPoint.Y * (float)ColorPalleteBox.Height / ((Bitmap)ColorPalleteBox.Image).Height));
        ColorPalleteBox.Invalidate();
    }

    private void ColorCodeTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (CheckKeyInput(e))
        {
            Color color = ColorUtils.GetColorFromColorCode(colorCodeTextBox.Text);
            if (color != Color.Empty)
            {
                UpdateSelectedColor(color);
                SelectNextControl((Control)sender, true, true, true, true);
            }
        }
    }

    private static bool CheckKeyInput(KeyEventArgs e)
    {
        Keys keys = ModifierKeys;
        return e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab || (keys == Keys.Shift && e.KeyCode == Keys.Tab);
    }
}