﻿using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;
using HoloToolkit.Unity.Receivers;
using HoloToolkit.Unity.InputModule;
using HoloToolkit.Unity.Buttons;
using UnityEngine.Video;
using System.Threading.Tasks;
using Assets.Models;
using Assets;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using System.Collections;

namespace HoloToolkit.Unity.Examples
{
    public class ButtonReceiver : InteractionReceiver
    {
        private string temp_id = "";
        public static List<Texture2D> pdfPages = new List<Texture2D>();
        public static int currentPage { get; set; }
              

        // Handles the inputs of all menu buttons
        protected override void InputUp(GameObject obj, InputEventData eventData)
        {
            Debug.Log(gameObject.tag);

            // Store the object name (productId/accountId)
            temp_id = obj.name;

            // Check if menu is a product or account menu
            if (gameObject.tag == "ProductMenu")
            {
                // Scale and create a new customer menu when a button is pressed (based on productId)
                transform.localScale = new Vector3(0, 0, 0);
                gameObject.GetComponent<MenuManager>().destroyCurrentMenu();
                gameObject.GetComponent<MenuManager>().CreateCustomerMenu(temp_id).GetAwaiter();
                transform.localScale = new Vector3(1f, 1f, 1f);
            }
            else if (gameObject.tag == "CustomerMenu")
            {
                // Separate the productId and the accountId
                string[] splitName = temp_id.Split('/');
                string productId = splitName[0];
                string accountId = splitName[1];

                // Handle testimony based on product and account id
                TestimonyHandler(productId, accountId).GetAwaiter();
                Debug.Log(obj.name);
            }
        }

        // handles testimonies
        public async Task TestimonyHandler(string productId, string accountId)
        {
            // Populate the list with holoinfo based on productId and accountId
            List<Info> infoList = await DataController.getHoloInfoByIds(productId, accountId);
            pdfPages.Clear();
            // Iterate through the infoList
            foreach(Info i in infoList)
            {
                // DummyData for testing

                // Check what of what type the testimony is (798200000 = Document, 798200001 = Video)
                if (i.infoType == "798200000")
                {
                    Document infoDocs = await DataController.getDocumentByInfoId(i.documentid);

                    foreach(string url in infoDocs.pageUrl)
                    {
                        StartCoroutine(loadImageFromUrl(url));
                    }
                }
                else if(i.infoType == "798200001")
                {
                    // Check wheter the url is a youtube link or a absolute video link
                    if (i.infoUrl.Contains("youtube"))
                    {
                        // Extract the video ID and play the youtube video
                        GameObject.Find("YoutubePlayer").GetComponent<SimplePlayback>().PlayerPause();
                        GameObject.Find("YoutubePlayer").GetComponent<SimplePlayback>().PlayYoutubeVideo(GetYouTubeVideoId(i.infoUrl));
                        GameObject.Find("VideoPlayers").transform.localScale = new Vector3(1, 1f, 0.01f);
                        GameObject.Find("YoutubePlayer").transform.localScale = new Vector3(1f, 0.58f, 0.01f);
                    }
                    else if(i.infoUrl.EndsWith(".mp4") || i.infoUrl.EndsWith(".flv") || i.infoUrl.EndsWith(".avi"))
                    {
                        // Start the video
                        GameObject.Find("VideoPlayers").transform.localScale = new Vector3(1f, 1f, 0.01f);
                        GameObject.Find("Screen").transform.localScale = new Vector3(1f, 0.58f, 0.01f);
                        StartCoroutine(GameObject.Find("Video").GetComponent<VideoBehaviour>().PlayVideo(i.infoUrl));
                    }
                    // Show warning if url is false
                    else
                    {
                        // what if wrong link
                    }
                }
                else
                {
                    // if no testimony
                }
            }
        }

        // Regex to extract the VideoId from a youtube url
        public string GetYouTubeVideoId(string url)
        { 
            var youtubeMatch =
                new Regex(@"youtu(?:\.be|be\.com)/(?:.*v(?:/|=)|(?:.*/)?)([a-zA-Z0-9-_]+)")
                .Match(url);
            return youtubeMatch.Success ? youtubeMatch.Groups[1].Value : string.Empty;
        }

        IEnumerator loadImageFromUrl(string url)
        {
            if (url.ToLower().EndsWith(".jpg"))
            {
                Texture2D urlLogo = new Texture2D(4, 4, TextureFormat.DXT1, false);
                using (WWW www = new WWW(url))
                {
                    // Retrieve the image 
                    yield return www;

                    // Convert the image to a texture
                    www.LoadImageIntoTexture(urlLogo);
                    pdfPages.Add(urlLogo);

                    GameObject.Find("PdfView").GetComponent<RawImage>().texture = pdfPages[0];

                    GameObject.Find("DocumentViewer").transform.localScale = new Vector3(0.8f, 0.8f, 1);
                    currentPage = 0;
                }
            }
        }
    }
}