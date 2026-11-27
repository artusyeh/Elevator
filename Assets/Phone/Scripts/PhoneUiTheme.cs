using UnityEngine;
[CreateAssetMenu(menuName="Phone/UI Theme")]
public sealed class PhoneUiTheme : ScriptableObject
{
    public Font font;
    public Color background=new Color(.043f,.059f,.071f);
    public Color surface=new Color(.078f,.102f,.118f);
    public Color text=new Color(.91f,.94f,.95f);
    public Color muted=new Color(.53f,.61f,.65f);
    public Color accent=new Color(.25f,.78f,.64f);
    public int bodySize=34;
}
