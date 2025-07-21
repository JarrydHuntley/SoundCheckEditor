using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Previews
{
    [JsonProperty("preview-lq-mp3")]//JSON �ʵ� �̸�(preview-lq-mp3)�� C# ������Ƽ �̸�(preview_lq_mp3)�� �ٸ��Ƿ�, JsonProperty ��Ʈ����Ʈ�� ����Ͽ� ����
    public string preview_lq_mp3 { get; set; } // ��ǰ�� MP3

    [JsonProperty("preview-hq-mp3")]
    public string preview_hq_mp3 { get; set; } // ��ǰ�� MP3

    [JsonProperty("preview-lq-ogg")]
    public string preview_lq_ogg { get; set; } // ��ǰ�� OGG

    [JsonProperty("preview-hq-ogg")]
    public string preview_hq_ogg { get; set; } // ��ǰ�� OGG
}
