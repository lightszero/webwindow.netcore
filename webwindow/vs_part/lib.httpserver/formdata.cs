using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace light.http.server
{
    public class FormData
    {
        public Dictionary<string, string> mapParams = new Dictionary<string, string>();
        public Dictionary<string, byte[]> mapFiles = new Dictionary<string, byte[]>();
        public static async Task<string> GetStringFromRequest(Microsoft.AspNetCore.Http.HttpRequest request)
        {
            try
            {
                byte[] allfile = null;
                int seek = 0;
                var _clen = request.Headers["Content-Length"];
                string clen = null;
                if (_clen.Count > 0)
                    clen = _clen[0];
                if (clen != null)
                {
                    int leng = int.Parse(clen);
                    allfile = new byte[leng];

                    while (request.Body.CanRead)
                    {
                        int read = await request.Body.ReadAsync(allfile, seek, leng - seek);
                        seek += read;
                        if (read == 0) break;
                    }
                }
                else
                {
                    allfile = new byte[4 * 1024 * 1024];

                    while (request.Body.CanRead)
                    {
                        int read = await request.Body.ReadAsync(allfile, seek, 1024);
                        seek += read;
                        if (read == 0) break;
                    }
                }


                string text = System.Text.Encoding.UTF8.GetString(allfile, 0, seek);
                return text;
            }
            catch
            {
                return null;
            }
        }
        public static async Task<FormData> FromRequest(Microsoft.AspNetCore.Http.HttpRequest request)
        {

            try
            {
                FormData data = new FormData();
                foreach (var kv in request.Query)
                {
                    data.mapParams[kv.Key] = kv.Value[0];
                }
                if (request.Method.ToUpper() == "POST")
                {
                    if (request.ContentType == null)
                    {
                        return data;
                    }
                    else if (request.ContentType == "application/x-www-form-urlencoded")
                    {
                        byte[] allfile = null;
                        int seek = 0;
                        var _clen = request.Headers["Content-Length"];
                        string clen = null;
                        if (_clen.Count > 0)
                            clen = _clen[0];
                        if (clen != null)
                        {
                            int leng = int.Parse(clen);
                            allfile = new byte[leng];

                            while (request.Body.CanRead)
                            {
                                int read = await request.Body.ReadAsync(allfile, seek, leng - seek);
                                seek += read;
                                if (read == 0) break;
                            }
                        }
                        else
                        {
                            allfile = new byte[4 * 1024 * 1024];

                            while (request.Body.CanRead)
                            {
                                int read = await request.Body.ReadAsync(allfile, seek, 1024);
                                seek += read;
                                if (read == 0) break;
                            }
                        }


                        string text = System.Text.Encoding.UTF8.GetString(allfile, 0, seek);
                        var infos = text.Split(new char[] { '=', '&' });
                        for (var i = 0; i < infos.Length / 2; i++)
                        {
                            data.mapParams[infos[i * 2]] = Uri.UnescapeDataString(infos[i * 2 + 1]);
                        }
                    }
                    else if (request.ContentType.IndexOf("multipart/form-data;") == 0)
                    {

                        byte[] allfile = null;
                        int seek = 0;
                        var _clen = request.Headers["Content-Length"];
                        string clen = null;
                        if (_clen.Count > 0)
                            clen = _clen[0];
                        if (clen != null)
                        {
                            int leng = int.Parse(clen);
                            allfile = new byte[leng];

                            while (request.Body.CanRead)
                            {
                                int read = await request.Body.ReadAsync(allfile, seek, leng - seek);
                                seek += read;
                                if (read == 0) break;
                            }
                        }
                        else
                        {
                            allfile = new byte[4 * 1024 * 1024];

                            while (request.Body.CanRead)
                            {
                                int read = await request.Body.ReadAsync(allfile, seek, 1024);
                                seek += read;
                                if (read == 0) break;
                            }
                        }


                        var iSplitTag = request.ContentType.IndexOf("=") + 1;
                        var sSplitstr = request.ContentType.Substring(iSplitTag);
                        sSplitstr = sSplitstr.Replace("\"", "");
                        var sSplitTag = "--" + sSplitstr;
                        var bSplitTag = System.Text.Encoding.ASCII.GetBytes(sSplitTag);

                        int iTag = ByteIndexOf(allfile, seek, bSplitTag, 0);
                        if (iTag < 0)
                        {
                            string s = System.Text.Encoding.ASCII.GetString(allfile, 0, seek);
                        }
                        else
                        {
                            while (iTag >= 0)
                            {
                                int iTagNext = ByteIndexOf(allfile, seek, bSplitTag, iTag + 1);
                                if (iTagNext < 0)
                                    break;
                                var bs = System.Text.Encoding.ASCII.GetBytes("\r\n\r\n");
                                int iStart = iTag + bSplitTag.Length + 2;
                                int iDataStart = ByteIndexOf(allfile, seek, bs, iStart) + 4;
                                string s = System.Text.Encoding.ASCII.GetString(allfile, iStart, iDataStart - iStart);
                                List<string> infos = new List<string>(s.Split(new string[] { "; ", ": ", "\r\n", "=" }, StringSplitOptions.None));
                                var i = infos.IndexOf("name");
                                var name = infos[i + 1].Substring(1);
                                name = name.Substring(0, name.Length - 1);

                                byte[] ddata = new byte[iTagNext - iDataStart - 2];
                                Array.Copy(allfile, iDataStart, ddata, 0, ddata.Length);
                                if (infos.Contains("application/octet-stream")
                                    || infos.Contains("image/png")
                                    || infos.Contains("image/jpg")
                                    || infos.Contains("image/jpeg")
                                    )
                                {
                                    data.mapFiles[name] = ddata;
                                }
                                else
                                {
                                    string txtData = System.Text.Encoding.UTF8.GetString(ddata);

                                    data.mapParams[name] = Uri.UnescapeDataString(txtData);
                                }
                                iTag = iTagNext;
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
                return data;
            }
            catch
            {
                return null;
            }
        }
        public static int ByteIndexOf(byte[] searched, int searchedlenght, byte[] find, int start)
        {
            bool matched = false;
            int end = find.Length - 1;
            int skip = 0;
            for (int index = start; index <= searchedlenght - find.Length; ++index)
            {
                matched = true;
                if (find[0] != searched[index] || find[end] != searched[index + end]) continue;
                else skip++;
                if (end > 10)
                    if (find[skip] != searched[index + skip] || find[end - skip] != searched[index + end - skip])
                        continue;
                    else skip++;
                for (int subIndex = skip; subIndex < find.Length - skip; ++subIndex)
                {
                    if (find[subIndex] != searched[index + subIndex])
                    {
                        matched = false;
                        break;
                    }
                }
                if (matched)
                {
                    return index;
                }
            }
            return -1;
        }

    }

}
