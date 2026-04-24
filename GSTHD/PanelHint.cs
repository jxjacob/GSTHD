namespace GSTHD
{
    internal interface PanelHint
    {
        void UpdateFromSettings();

        string Name { get; set; }
        int PlacedOrder { get; set; }

        LabelExtended LabelPlace { get; set; }
    }
}
