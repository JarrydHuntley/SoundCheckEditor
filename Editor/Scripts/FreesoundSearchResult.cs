using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static SoundCheckEditor;

public class FreesoundSearchResult//Freesound API�� �˻� ����� ǥ���� Ŭ����
{    
    public string next { get; set; }//���� ������ url
    public string previous { get; set; }//���� ������ url
    public List<FreesoundSound> results { get; set; }//�˻� ��� ����Ʈ

}