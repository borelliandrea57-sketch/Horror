using Godot;
using System.Collections.Generic;
public partial class GameRoot {
 private static readonly Dictionary<string,Vector2[]> anchors84=new(){
        {"bell",new Vector2[]{new(0.7833f,0.3840f),new(0.1812f,0.3920f),new(0.8542f,0.7160f),new(0.1264f,0.7040f)}},
        {"corrupted",new Vector2[]{new(0.7905f,0.3397f),new(0.2027f,0.3397f),new(0.6419f,0.7115f),new(0.3851f,0.7147f)}},
        {"falciante84",new Vector2[]{new(0.7784f,0.2769f),new(0.2324f,0.2508f),new(0.6649f,0.6775f),new(0.3622f,0.6840f)}},
        {"ossuary",new Vector2[]{new(0.8066f,0.4614f),new(0.3188f,0.5943f),new(0.8449f,0.6943f),new(0.5610f,0.6986f)}},
        {"sexton",new Vector2[]{new(0.7653f,0.3569f),new(0.2069f,0.3401f),new(0.8167f,0.6902f),new(0.0972f,0.6296f)}},
        {"stitch",new Vector2[]{new(0.7500f,0.2269f),new(0.2500f,0.2500f),new(0.8111f,0.6500f),new(0.1514f,0.6231f)}},
        {"z74_cyst",new Vector2[]{new(0.8103f,0.2584f),new(0.2069f,0.2450f),new(0.6667f,0.6913f),new(0.3908f,0.6913f)}},
        {"z74_eye",new Vector2[]{new(0.7853f,0.2980f),new(0.2251f,0.2748f),new(0.6649f,0.6854f),new(0.3508f,0.6854f)}},
        {"z74_flayed",new Vector2[]{new(0.7784f,0.2769f),new(0.2378f,0.2410f),new(0.6649f,0.6775f),new(0.3622f,0.6840f)}},
        {"z74_heavy",new Vector2[]{new(0.7870f,0.2550f),new(0.2083f,0.2748f),new(0.7037f,0.7053f),new(0.3194f,0.7119f)}},
        {"z74_hunch",new Vector2[]{new(0.8439f,0.3446f),new(0.1854f,0.3670f),new(0.7073f,0.6929f),new(0.3805f,0.6929f)}},
        {"z74_thin",new Vector2[]{new(0.8054f,0.2370f),new(0.2483f,0.2435f),new(0.6577f,0.6396f),new(0.3490f,0.6331f)}},
 };
 private Vector2 WoundAnchor84(string k,int i)=>anchors84.TryGetValue(k,out var a)?a[i-2]:new Vector2(i%2==0?.7f:.3f,i<4?.28f:.65f);
}
