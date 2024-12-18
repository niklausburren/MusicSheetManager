namespace MusicSheetManager.Models;

public class MusicSheet
{
    public string Title { get; set; }
    public string Composer { get; set; }
    public string Arranger { get; set; }
    public InstrumentInfo Instrument { get; set; }
    public VoiceInfo Voice { get; set; }

}
