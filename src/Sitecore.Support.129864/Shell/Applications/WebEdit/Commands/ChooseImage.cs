using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Sitecore.Shell.Framework.Commands;
using Sitecore.Diagnostics;
using Sitecore.Shell.Applications.WebEdit.Commands;
using Sitecore.Web;
using Sitecore.Web.UI.Sheer;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Shell.Applications.ContentEditor;
using Sitecore.Shell.Applications.Dialogs.MediaBrowser;
using Sitecore.Exceptions;

namespace Sitecore.Support.Shell.Applications.WebEdit.Commands
{
  public class ChooseImage : Sitecore.Shell.Applications.WebEdit.Commands.ChooseImage
  {
    public override void Execute(CommandContext context)
    {
      Assert.ArgumentNotNull(context, "context");
      WebEditCommand.ExplodeParameters(context);
      string formValue = WebUtil.GetFormValue("scPlainValue");
      context.Parameters.Add("fieldValue", formValue);
      Context.ClientPage.Start(this, "Run", context.Parameters);
    }

    protected static new void Run(ClientPipelineArgs args)
    {
      Assert.ArgumentNotNull(args, "args");
      Item itemNotNull = Client.GetItemNotNull(args.Parameters["itemid"], Language.Parse(args.Parameters["language"]));
      itemNotNull.Fields.ReadAll();
      Field field = itemNotNull.Fields[args.Parameters["fieldid"]];
      Assert.IsNotNull(field, "field");
      ImageField imageField = new ImageField(field, field.Value);
      string str = args.Parameters["controlid"];
      string text = args.Parameters["fieldValue"];
      if (args.IsPostBack)
      {
        if (args.Result != "undefined")
        {
          string value;
          if (!string.IsNullOrEmpty(args.Result))
          {
            MediaItem mediaItem = Client.ContentDatabase.GetItem(args.Result);
            if (mediaItem != null)
            {
              imageField.SetAttribute("mediaid", mediaItem.ID.ToString());
              if (text.Length > 0)
              {
                XmlValue xmlValue = new XmlValue(text, "image");
                string attribute = xmlValue.GetAttribute("height");
                if (!string.IsNullOrEmpty(attribute))
                {
                  imageField.Height = attribute;
                }
                string attribute2 = xmlValue.GetAttribute("width");
                if (!string.IsNullOrEmpty(attribute2))
                {
                  imageField.Width = attribute2;
                }
              }
            }
            else
            {
              SheerResponse.Alert("Item not found.", new string[0]);
            }
            value = imageField.Value;
          }
          else
          {
            value = string.Empty;
          }
          string value2 = WebEditImageCommand.RenderImage(args, value);
          SheerResponse.SetAttribute("scHtmlValue", "value", value2);
          SheerResponse.SetAttribute("scPlainValue", "value", value);
          SheerResponse.Eval("scSetHtmlValue('" + str + "')");
          return;
        }
      }
      else
      {
        string text2 = StringUtil.GetString(new string[]
        {
      field.Source,
      "/sitecore/media library"
        });
        if (text.Length > 0)
        {
          XmlValue xmlValue2 = new XmlValue(text, "image");
          text = xmlValue2.GetAttribute("mediaid");
        }
        string text3 = text;
        if (text2.StartsWith("~", System.StringComparison.InvariantCulture))
        {
          if (string.IsNullOrEmpty(text3))
          {
            text3 = StringUtil.Mid(text2, 1);
          }
          text2 = "/sitecore/media library";
        }
        Language language = itemNotNull.Language;
        MediaBrowserOptions mediaBrowserOptions = new MediaBrowserOptions();
        Item item = Client.ContentDatabase.GetItem(text2, language);
        if (item == null)
        {
          throw new ClientAlertException("The source of this Image field points to an item that does not exist.");
        }
        mediaBrowserOptions.Root = item;
        mediaBrowserOptions.AllowEmpty = true;
        if (!string.IsNullOrEmpty(text3))
        {
          Item item2 = Client.ContentDatabase.GetItem(text3, language);
          if (item2 != null)
          {
            mediaBrowserOptions.SelectedItem = item2;
          }
        }
        SheerResponse.ShowModalDialog(mediaBrowserOptions.ToUrlString().ToString(), "1200px", "700px", string.Empty, true);
        args.WaitForPostBack();
      }
    }
  }
}