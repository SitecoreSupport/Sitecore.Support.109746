using Sitecore.Configuration;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Forms.Core.Data;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Links;
using Sitecore.Resources.Media;
using Sitecore.Text;
using Sitecore.Web;
using Sitecore.WFFM.Abstractions.Actions;
using Sitecore.WFFM.Abstractions.Data;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Sitecore.SecurityModel;

namespace Sitecore.Support.Forms.Mvc.Models
{
    public class FormModel : IFormModel, IModelEntity, ICloneable
    {
        public IFormItem Item
        {
            get;
            private set;
        }

        public List<ExecuteResult.Failure> Failures
        {
            get;
            set;
        }

        public string SuccessRedirectUrl
        {
            get;
            set;
        }

        public bool RedirectOnSuccess
        {
            get;
            set;
        }

        public DateTime RenderedTime
        {
            get;
            set;
        }

        public bool ReadQueryString
        {
            get;
            set;
        }

        public NameValueCollection QueryParameters
        {
            get;
            set;
        }

        public int EventCounter
        {
            get;
            set;
        }

        public List<ControlResult> Results
        {
            get;
            set;
        }

        public bool IsValid
        {
            get;
            set;
        }

        public Guid UniqueId
        {
            get;
            private set;
        }

        public FormModel(Guid uniqueId, Item item) : this(uniqueId)
        {
            Assert.ArgumentNotNull(item, "item");
            this.Item = new FormItem(item);
            this.RedirectOnSuccess = this.Item.SuccessRedirect;
            this.IsValid = true;
            if (this.RedirectOnSuccess)
            {
                LinkField successPage = this.Item.SuccessPage;
                if (successPage != null)
                {
                    UrlString urlString = null;
                    if (successPage.LinkType == "external")
                    {
                        urlString = new UrlString(successPage.Url);
                    }
                    else
                    {
                        if (successPage.TargetItem == null)
                        {
                            // Sitecore.Support.109746
                            Log.Error("[WFFM] [Sitecore.Support.109746] Redirect item is null", new NullReferenceException(), this);
                            //throw new NullReferenceException("Redirect item is null");
                        }
                        string linkType;
                        if ((linkType = successPage.LinkType) != null)
                        {
                            if (linkType != "internal" && linkType == "media")
                            {
                                urlString = new UrlString(MediaManager.GetMediaUrl(new MediaItem(successPage.TargetItem)));
                            }
                            else if (successPage.TargetID.IsNull == false)
                            {
                                /*UrlOptions defaultUrlOptions = LinkManager.GetDefaultUrlOptions();
                                defaultUrlOptions.SiteResolving = Sitecore.Configuration.Settings.Rendering.SiteResolving;
                                urlString = new UrlString(LinkManager.GetItemUrl(successPage.TargetItem, defaultUrlOptions));*/

                                Item newItem;
                                UrlOptions defaultUrlOptions = LinkManager.GetDefaultUrlOptions();
                                defaultUrlOptions.SiteResolving = Settings.Rendering.SiteResolving;
                                using (new SecurityDisabler())
                                {
                                    newItem = item.Database.Items[successPage.TargetID];
                                }
                                urlString = new UrlString(LinkManager.GetItemUrl(newItem, defaultUrlOptions));
                            }
                        }
                    }
                    if (urlString == null)
                    {
                        return;
                    }
                    string queryString = this.Item.SuccessPage.QueryString;
                    if (!string.IsNullOrEmpty(queryString))
                    {
                        urlString.Parameters.Add(WebUtil.ParseUrlParameters(queryString));
                    }
                    this.SuccessRedirectUrl = urlString.ToString();
                }
            }
        }

        public FormModel(Guid uniqueId)
        {
            Assert.ArgumentCondition(uniqueId != Guid.Empty, "uniqueId", "uniqueId is empty");
            this.UniqueId = uniqueId;
            this.Results = new List<ControlResult>();
            this.Failures = new List<ExecuteResult.Failure>();
        }

        public object Clone()
        {
            FormModel formModel = (FormModel)base.MemberwiseClone();
            formModel.Results = new List<ControlResult>();
            formModel.Failures = new List<ExecuteResult.Failure>();
            return formModel;
        }
    }
}