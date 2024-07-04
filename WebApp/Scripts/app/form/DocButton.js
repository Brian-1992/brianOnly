/***
 
.x-attachment-btn {
   border-color: #6262e5 !important;
}

 ***/ //Required CSS

//定義DocButton元件
Ext.define('WEBAPP.form.DocButton', {
    extend: 'Ext.Button',
    alias: 'widget.docbutton',

    /*******************************
    ** 公有屬性，可在config設定
    ********************************/
    cls: 'x-attachment-btn',
    disabled: true,
    documentKey: null,

    /*******************************
    ** 私有屬性，勿設定或使用
    ********************************/
    uploadKey: null,

    //DocButton元件初始化
    initComponent: function (config) {
        Ext.apply(this, config);
        Ext.apply(this, this.getDefaultConfig());
        this.callParent(arguments);
    },

    getDefaultConfig: function () {
        return {
            listeners: {
                click: function (c) {
                    var f = $('<input/>').attr('name', 'UK').attr('value', c.uploadKey);
                    $('<form/>').attr('action', '/api/File/Download')
                        .attr('method', 'POST').append(f)
                        .appendTo('body').submit().remove();
                },
                afterrender: function (c) {
                    var docButton = c;
                    var ajaxRequest = $.ajax({
                        type: "POST",
                        url: '/api/UR1016/GetUploadKey',
                        dataType: "json",
                        data: { DK: this.documentKey }
                    })
                        .done(function (data, textStatus) {
                            docButton.uploadKey = data;
                            docButton.setDisabled(
                                (docButton.uploadKey == null ||
                                    docButton.uploadKey == ''));
                        })
                        .fail(function (data, textStatus) {
                        });
                }
            }
        };
    }
});