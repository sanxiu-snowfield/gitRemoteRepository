using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace WpfApp4
{
    public class TTSUtility
    {
        string ServerAdress { get; set; }
        string ExampleWav { get; set; }
        string PromptText { get; set; }
        //最多同时tts数
        int MaxRequestNum { get; set; }
        //存储tts好的音频
        List<string> PlayCache { get; set; }
        //音频播放信号量
        static Semaphore semaphore = new Semaphore(1, 1);
        //语句间隔milsec
        int PhraseGap { get; set; }

        public TTSUtility()
        {
            MaxRequestNum = 5;
            ServerAdress = @"http://0.0.0.0:9880/";
            ExampleWav = "D:\\github\\GPT-SoVITS-v2-240821\\API_test\\体迅飞凫，飘忽若神，凌波微步，罗袜生尘。动无常则，若危若安.wav";
            PromptText = "体迅飞凫，飘忽若神，凌波微步，罗袜生尘。动无常则，若危若安";

            PlayCache = new List<string>();

            PhraseGap = 300;
        }


        int ReadedIndex { get; set; }
        public void TTSTxt(string path)
        {
            using (FileStream fileStream = new FileStream(path, FileMode.Open))
            {
                using (StreamReader reader = new StreamReader(fileStream))
                {
                    while (!reader.EndOfStream)
                    {
                        // 读取一个自然段（直到遇到换行符）
                        string paragraph = ReadParagraph(reader);
                        if (!string.IsNullOrEmpty(paragraph) && PlayCache.Count< MaxRequestNum)
                        {
                            TTSPhrase(paragraph);
                            PlayWavFile(PlayCache.Last());
                        }
                        else if(!string.IsNullOrEmpty(paragraph) && PlayCache.Count >= MaxRequestNum)
                        {
                            while (PlayCache.Count==5)
                            {
                                Thread.Sleep(100);
                            }
                            TTSPhrase(paragraph);
                            PlayWavFile(PlayCache.Last());
                        }
                    }
                    
                }
            }
        }

        // 读取一个自然段的方法
        static string ReadParagraph(StreamReader reader)
        {
            StringBuilder paragraph = new StringBuilder();
            string line;

            // 读取一行或直到遇到换行符
            while ((line = reader.ReadLine()) != null)
            {
                // 如果读取到的行不是空行，将其添加到段落中
                if (!string.IsNullOrWhiteSpace(line))
                {
                    paragraph.AppendLine(line);
                    break;
                }
            }

            return paragraph.ToString().TrimEnd(); // 返回读取的段落，并去除末尾的空白字符
        }

        public async void TTSPhrase(string phrase)
        {
            // 指定要请求的URL
            string url = $"{ServerAdress}?refer_wav_path={ExampleWav}&prompt_text={PromptText}&prompt_language=中文&text={phrase}&text_language=中文&top_k=15&top_p=1&temperature=1&speed=1";

            long guid = DateTime.Now.Ticks;
            // 指定本地文件路径
            string filePath = @$"..\wavCache\{guid}.wav";
            PlayCache.Add(filePath);

            try
            {
                using (HttpClient client = new HttpClient())
                {
                    // 发送GET请求
                    using (HttpResponseMessage response = await client.GetAsync(url))
                    {
                        // 确保响应状态码为200（成功）
                        response.EnsureSuccessStatusCode();

                        // 读取响应内容
                        byte[] audioData = await response.Content.ReadAsByteArrayAsync();

                        // 保存WAV文件
                        File.WriteAllBytes(filePath, audioData);
                    }

                }
            }
            catch (HttpRequestException e)
            {
                // 处理请求异常
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }

        }

        public void PlayWavFile(string path)
        {
            MediaPlayer mediaPlayer = new MediaPlayer();

            while (true)
            {
                if (File.Exists(path))
                {
                    Task.Run(() => {
                        semaphore.WaitOne();
                        mediaPlayer.Open(new Uri(path, UriKind.Relative));
                        mediaPlayer.Play();
                        Thread.Sleep(PhraseGap);
                        PlayCache.Remove(path);
                        Thread.Sleep(PhraseGap);

                        semaphore.Release();
                    });
                    break;
                }
                else
                {
                    Thread.Sleep(300);
                }
            }
           
        }

    }
}
