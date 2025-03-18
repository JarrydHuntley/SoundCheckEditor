using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class Previews
{
    [JsonProperty("preview-lq-mp3")]//JSON 필드 이름(preview-lq-mp3)과 C# 프로퍼티 이름(preview_lq_mp3)이 다르므로, JsonProperty 어트리뷰트를 사용하여 매핑
    public string preview_lq_mp3 { get; set; } // 저품질 MP3

    [JsonProperty("preview-hq-mp3")]
    public string preview_hq_mp3 { get; set; } // 고품질 MP3

    [JsonProperty("preview-lq-ogg")]
    public string preview_lq_ogg { get; set; } // 저품질 OGG

    [JsonProperty("preview-hq-ogg")]
    public string preview_hq_ogg { get; set; } // 고품질 OGG
}
