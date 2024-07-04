/***
 
.x-attachment-btn {
   border-color: #6262e5 !important;
}

 ***/ //Required CSS

//定義AttachmentWindow元件
Ext.define('WEBAPP.form.FileWindow', {
    extend: 'Ext.Window',

    /*******************************
    ** 公有屬性，可在config設定
    ********************************/
    viewport: null,
    content: null,
    title: '附件上傳管理',
    width: 800,
    height: 450,

    /*******************************
    ** 私有屬性，勿設定或使用
    ********************************/

    //AttachmentWindow元件初始化
    initComponent: function () {
        Ext.apply(this, this.getDefaultConfig());
        this.callParent(arguments);
    },

    getDefaultConfig: function () {
        return {
            modal: true,
            layout: 'fit',
            autoScroll: true,
            closeAction: 'destroy',
            constrain: true,
            resizable: true,
            closable: false,
            winActWidth: this.width,
            winActHeight: this.height,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((this.viewport.width - this.winActWidth > 0) ? this.winActWidth : this.viewport.width - 36);
                    xwin.setHeight((this.viewport.height - this.winActHeight > 0) ? this.winActHeight : this.viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    this.winActWidth = width;
                    this.winActHeight = height;
                }
            }
        };
    }
});