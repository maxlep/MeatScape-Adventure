using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

public class CommentNode : Node
{
    [HideIf("$zoom")] [LabelWidth(175f)] [SerializeField] [Required]
    [TextArea(8, 10)]
    private string CommentText = "<Enter Comment Here>";

    public string GetCommentText() => CommentText;
}
