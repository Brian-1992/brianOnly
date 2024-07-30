Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "每月工作量報表";

    var nowDate = '';

    var reportUrl_1 = '/Report/A/AB0075_1.aspx';
    var reportUrl_2 = '/Report/A/AB0075_2.aspx';
    var reportUrl_3 = '/Report/A/AB0075_3.aspx';
    var reportUrl_4 = '/Report/A/AB0075_4.aspx';

    var GetWH_NO = '../../../api/AB0075/GetWH_NO';

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var WH_NO_Store = Ext.create('Ext.data.Store', {  //查詢庫房代碼的store
        fields: ['VALUE', 'TEXT']
    });

    function SetWH_NO() { //建立庫房代碼的下拉式選單
        Ext.Ajax.request({
            url: GetWH_NO,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            WH_NO_Store.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                            first_wh_no = wh_nos[0].VALUE;
                        }
                    }
                    T1Query.getForm().findField("P1").setValue(first_wh_no);
                }
            },
            failure: function (response, options) {
            }
        });
    }

    function SetDate() {
        nowDate = new Date();
        nowDate.getMonth();
        nowDate = Ext.Date.format(nowDate, "Ymd") - 19110000;
        nowDate = nowDate.toString().substring(0, 5);
        T1Query.getForm().findField('P0').setValue(nowDate);
    }

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: 70,
            width: 180
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'monthfield',
                    fieldLabel: '月份別',
                    name: 'P0',
                    id: 'P0',
                    width: 170,
                    labelWidth: 70,
                    enforceMaxLength: true, // 限制可輸入最大長度
                    padding: '0 4 0 4',
                    fieldCls: 'required',
                    allowBlank: false
                }, {
                    xtype: 'combo',
                    fieldLabel: '庫房別',
                    name: 'P1',
                    id: 'P1',
                    width: 250,
                    labelWidth: 70,
                    store: WH_NO_Store,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>',
                    allowBlank: false, // 欄位是否為必填
                    fieldCls: 'required',
                    blankText: "請輸入庫房別"
                },
                {
                    xtype: 'button',
                    itemId: 'query',
                    text: '查詢',
                    handler: function () {
                        if (T1Query.getForm().isValid() == false) {
                            if (T1Query.getForm().findField('P0').getValue() == null) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>月份別</span>為必填');
                            }
                            else if (T1Query.getForm().findField('P1').getValue() == null) {
                                Ext.Msg.alert('提醒', '<span style=\'color:red\'>庫房別</span>為必填');
                            }
                        }
                        else {
                            reportAutoGenerate(true);
                        }
                    }
                },
                {
                    xtype: 'button',
                    itemId: 'clean',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        T1Query.getForm().findField('P0').setValue(nowDate);
                        T1Query.getForm().findField("P1").setValue(first_wh_no);
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        //resizeTabs: true,
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            items: [T1Query]
        }]
    });

    function T1Load(clearMsg) {
        if (clearMsg) {
            msglabel('訊息區:');
        }
    }

    function reportAutoGenerate(autoGen) {
        if (autoGen) {
            if (T1Query.getForm().findField('P1').getValue() == 'PH1A' ||
                T1Query.getForm().findField('P1').getValue() == 'PH1R' ||
                T1Query.getForm().findField('P1').getValue() == 'PHMC' ||
                T1Query.getForm().findField('P1').getValue() == 'PH1C') {
                var p0 = T1Query.getForm().findField('P0').rawValue;
                var p1 = T1Query.getForm().findField('P1').rawValue;
                var qstring = 'MONTH=' + p0 + '&WH_NO=' + p1;
                Ext.getDom('mainContent').src = autoGen ? reportUrl_1 + '?' + qstring : '';
            }
            else if (T1Query.getForm().findField('P1').getValue() == 'CHEMO' ||
                T1Query.getForm().findField('P1').getValue() == 'CHEMOT') {
                var p0 = T1Query.getForm().findField('P0').rawValue;
                var p1 = T1Query.getForm().findField('P1').rawValue;
                var qstring = 'MONTH=' + p0 + '&WH_NO=' + p1;
                Ext.getDom('mainContent').src = autoGen ? reportUrl_2 + '?' + qstring : '';
            }
            else if (T1Query.getForm().findField('P1').getValue() == 'PCA') {
                var p0 = T1Query.getForm().findField('P0').rawValue;
                var p1 = T1Query.getForm().findField('P1').rawValue;
                var qstring = 'MONTH=' + p0 + '&WH_NO=' + p1;
                Ext.getDom('mainContent').src = autoGen ? reportUrl_3 + '?' + qstring : '';
            }
            else if (T1Query.getForm().findField('P1').getValue() == 'TPN') {
                var p0 = T1Query.getForm().findField('P0').rawValue;
                var p1 = T1Query.getForm().findField('P1').rawValue;
                var qstring = 'MONTH=' + p0 + '&WH_NO=' + p1;
                Ext.getDom('mainContent').src = autoGen ? reportUrl_4 + '?' + qstring : '';
            }
        }
    };

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [
            {
                itemId: 'tabReport',
                region: 'north',
                frame: false,
                layout: 'fit',
                collapsible: false,
                border: false,
                items: [T1Query]
            },
            {
                region: 'center',
                xtype: 'form',
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>'
            }
        ]
    });

    reportAutoGenerate(false);

    T1Query.getForm().findField('P0').focus();
    SetWH_NO();
    SetDate();
});
