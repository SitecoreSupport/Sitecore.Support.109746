using Sitecore.Data;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Forms.Mvc.Data.Wrappers;
using Sitecore.Forms.Mvc.Interfaces;
using Sitecore.Forms.Mvc.Models;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Web;

namespace Sitecore.Support.Forms.Mvc.Services
{
    public class FormRepository : IRepository<FormModel>
    {
        private readonly Dictionary<Guid, FormModel> models = new Dictionary<Guid, FormModel>();

        public IRenderingContext RenderingContext
        {
            get;
            private set;
        }

        public FormRepository(IRenderingContext renderingContext)
        {
            Assert.ArgumentNotNull(renderingContext, "renderingContext");
            this.RenderingContext = renderingContext;
        }

        public FormModel GetModel(Guid uniqueId)
        {
            if (uniqueId != Guid.Empty && this.models.ContainsKey(uniqueId))
            {
                return (FormModel)this.models[uniqueId].Clone();
            }
            string dataSource = this.RenderingContext.Rendering.DataSource;
            string text;
            if (!string.IsNullOrEmpty(dataSource) && ID.IsID(dataSource))
            {
                text = dataSource;
            }
            else
            {
                text = this.RenderingContext.Rendering.Parameters[Sitecore.Forms.Mvc.Constants.FormId];
            }
            if (!ID.IsID(text))
            {
                return null;
            }
            ID id = ID.Parse(text);
            Item item = this.RenderingContext.Database.GetItem(id);
            Assert.IsNotNull(item, "Form item is absent");
            Sitecore.Support.Forms.Mvc.Models.FormModel formModel = new Sitecore.Support.Forms.Mvc.Models.FormModel(uniqueId, item)
            {
                ReadQueryString = MainUtil.GetBool(this.RenderingContext.Rendering.Parameters[Sitecore.Forms.Mvc.Constants.ReadQueryString], false),
                QueryParameters = HttpUtility.ParseQueryString(WebUtil.GetQueryString())
            };
            // Sitecore.Support.109746
            FormModel newModel = Convert(formModel);
            this.models.Add(uniqueId, newModel);
            return newModel;
        }

        public FormModel GetModel()
        {
            return this.GetModel(this.RenderingContext.Rendering.UniqueId);
        }

        // Sitecore.Support.109746
        private static FormModel Convert(Sitecore.Support.Forms.Mvc.Models.FormModel formModel)
        {
            FormModel model = new FormModel(formModel.UniqueId)
            {
                IsValid = formModel.IsValid,
                EventCounter = formModel.EventCounter,
                QueryParameters = formModel.QueryParameters,
                ReadQueryString = formModel.ReadQueryString,
                RedirectOnSuccess = formModel.RedirectOnSuccess,
                RenderedTime = formModel.RenderedTime,
                Results = formModel.Results,
                SuccessRedirectUrl = formModel.SuccessRedirectUrl
            };
            model.GetType().GetProperty("Item").SetValue(model, formModel.Item, null);
            return model;
        }
    }
}