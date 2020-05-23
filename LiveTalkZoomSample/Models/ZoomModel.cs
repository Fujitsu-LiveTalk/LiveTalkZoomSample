/*
 * Copyright 2020 FUJITSU SOCIAL SCIENCE LABORATORY LIMITED
 * クラス名　：ZoomModel
 * 概要      ：Zoomの字幕APIをREST POSTで呼び出す
*/
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LiveTalkZoomSample.Models
{
    /// <summary>
    /// https://support.zoom.us/hc/en-us/articles/115002212983
    /// </summary>
    internal class ZoomModel : INotifyPropertyChanged
    {
        private BlockingCollection<string> Queue = new BlockingCollection<string>();
        private CancellationTokenSource TokenSource = new CancellationTokenSource();
        private string LastAccessTokenError = null;
        private long SeqNo = 1;
        private string LangCode = "ja-JP";

        /// <summary>
        /// APIトークン
        /// </summary>
        private string _APIToken = string.Empty;
        public string APIToken
        {
            get { return this._APIToken; }
            internal set
            {
                if (this._APIToken != value)
                {
                    this._APIToken = value;
                    OnPropertyChanged();
                    Common.Config.SetConfig("APIToken", value);
                }
            }
        }

        /// <summary>
        /// 連携ファイル名
        /// </summary>
        private string _FileName = string.Empty;
        public string FileName
        {
            get { return this._FileName; }
            internal set
            {
                if (this._FileName != value)
                {
                    this._FileName = value;
                    OnPropertyChanged();
                    Common.Config.SetConfig("FileName", value);
                }
            }
        }

        /// <summary>
        /// 処理中メッセージ
        /// </summary>
        private string _Message = string.Empty;
        public string Message
        {
            get { return this._Message; }
            internal set
            {
                if (this._Message != value)
                {
                    this._Message = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// True:発話者名も字幕に表示する
        /// </summary>
        private bool _IsWithName = false;
        public bool IsWithName
        {
            get { return this._IsWithName; }
            internal set
            {
                if (this._IsWithName != value)
                {
                    this._IsWithName = value;
                    OnPropertyChanged();
                }
            }
        }

        #region "PROXY"
        /// <summary>
        /// 認証PROXYならIDを指定
        /// </summary>
        private string _ProxyId = string.Empty;
        public string ProxyId
        {
            get { return this._ProxyId; }
            internal set
            {
                if (this._ProxyId != value)
                {
                    this._ProxyId = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// 認証PROXYならパスワードを指定
        /// </summary>
        private string _ProxyPassword = string.Empty;
        public string ProxyPassword
        {
            get { return this._ProxyPassword; }
            internal set
            {
                if (this._ProxyPassword != value)
                {
                    this._ProxyPassword = value;
                    OnPropertyChanged();
                }
            }
        }
        #endregion

        public ZoomModel()
        {
            this.APIToken = Common.Config.GetConfig("APIToken");
            this.FileName = Common.Config.GetConfig("FileName");
            this.SendZoom();
        }

        /// <summary>
        /// Zoomに字幕を送信する
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        internal void SendMessage(string s)
        {
            this.Queue.Add(s);
        }

        // Zoomへ字幕送信(１度に送信できるのは144文字)
        private void SendZoom()
        {
            // 字幕キュー処理
            Task.Factory.StartNew(async () =>
            {
                while (true)
                {
                    // 音声合成の再生
                    if (Queue.TryTake(out string s, -1, TokenSource.Token))
                    {
                        var reg = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");
                        var items = reg.Split(s);
                        var messageTime = DateTime.Parse(items[0].Substring(1, items[0].Length - 2));
                        var name = items[1].Substring(1, items[1].Length - 2);
                        var message = items[2].Substring(1, items[2].Length - 2);
                        var translateText = items[3].Substring(1, items[3].Length - 2);

                        if (!string.IsNullOrEmpty(translateText)) message = translateText;
                        this.Message = $"{messageTime} {message}";
                        using (var client = new HttpClient(SetProxy()))
                        {
                            try
                            {
                                var sendMessage = this.IsWithName ? $"[{name}]{Environment.NewLine}{message}{Environment.NewLine}" : $"{message}{Environment.NewLine}";
                                using (var request = new HttpRequestMessage())
                                {
                                    request.Method = HttpMethod.Post;
                                    request.RequestUri = new Uri(this.APIToken + $"&seq={this.SeqNo++}&lang={this.LangCode}");
                                    request.Content = new StringContent(sendMessage, Encoding.UTF8, "text/plain");
                                    request.Headers.Add("Connection", "close");
                                    client.Timeout = TimeSpan.FromSeconds(30);
                                    using (var response = await client.SendAsync(request))
                                    {
                                        response.EnsureSuccessStatusCode();
                                    }
                                }
                                this.LastAccessTokenError = "";
                            }
                            catch (Exception ex)
                            {
                                if (ex.Message != this.LastAccessTokenError)
                                {
                                    System.Diagnostics.Debug.WriteLine(ex.Message);
                                    this.LastAccessTokenError = ex.Message;
                                    throw ex; // エラー通知（通知方法はMainViewModelで規定）
                                }
                            }
                        }
                    }
                }
            });
        }
        private HttpClientHandler SetProxy()
        {
            var ch = new HttpClientHandler() { UseCookies = true };
            ch.Proxy = System.Net.WebRequest.DefaultWebProxy;
            ch.Proxy.Credentials = new System.Net.NetworkCredential(this.ProxyId, this.ProxyPassword);
            return ch;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName]String propertyName = "")
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
