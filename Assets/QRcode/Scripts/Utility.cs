using System;
using System.Text.RegularExpressions;
using UnityEngine;

public class Utility
{
	public static bool CheckIsUrlFormat(string strValue)
	{
		return Utility.CheckIsFormat("(http://)?([\\w-]+\\.)+[\\w-]+(/[\\w- ./?%&=]*)?", strValue);
	}

	public static bool CheckIsFormat(string strRegex, string strValue)
	{
		if (strValue != null && strValue.Trim() != string.Empty)
		{
			Regex regex = new Regex(strRegex);
			return regex.IsMatch(strValue);
		}
		return false;
	}



	//Converter

	public static Sprite Texture2DToSprite(Texture2D texture)
	{
		var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100.0f);
		return sprite;
	}
}
