public class ElevenLabsRequest
{
        public string text;
        public string model_id = "eleven_flash_v2_5";
        public string language_code = "no";

        public ElevenLabsRequest(string text, string model_id = "eleven_flash_v2_5", string language_code = "no")
        {
            this.text = text;
            this.model_id = model_id;
            this.language_code = language_code;    
        }
}
