using System.Windows;
using System.Windows.Controls;
using Newtonsoft.Json;

namespace ChatUI
{
	public class ChatDataTemplateSelector : DataTemplateSelector
	{
		public DataTemplate TextTemplate { get; set; }
		public DataTemplate ImageTemplate { get; set; }
		public DataTemplate FileTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var output = (item as ReceiveMessage);
            // 檢查 item 的屬性值，並返回適當的 DataTemplate

            switch (output.Type) 
            {
                case "Image":
                    return ImageTemplate;
                case "Text":
                    return TextTemplate;
                case "File":
                    return FileTemplate;
            }
                
            

            // 如果沒有符合的條件，返回預設的 DataTemplate
            return base.SelectTemplate(item, container);
        }
    }
}
