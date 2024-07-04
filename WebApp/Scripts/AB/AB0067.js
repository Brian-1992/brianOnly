﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var reportUrl = '/Report/A/AB0067.aspx';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var ApplyWH_NOComboGet = '../../../api/AB0067/GetApplyWH_NOComboOne';
    var WriteOffWH_NOComboGet = '../../../api/AB0067/GetWriteOffWH_NOComboOne';

    // 申請庫別
    var Applywh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });
    // 銷帳庫別
    var WriteOffwh_noStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: ApplyWH_NOComboGet,
            params: { limit: 10, page: 1, start: 0 },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;

                    if (tb_wh_no.length > 0) {
                        for (var i = 0; i < tb_wh_no.length; i++) {
                            Applywh_noStore.add({ WH_NO: tb_wh_no[i].VALUE, WH_NAME: tb_wh_no[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

        Ext.Ajax.request({
            url: WriteOffWH_NOComboGet,
            params: { limit: 10, page: 1, start: 0 },

            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);

                if (data.success) {
                    var tb_wh_no = data.etts;

                    if (tb_wh_no.length > 0) {
                        for (var i = 0; i < tb_wh_no.length; i++) {
                            WriteOffwh_noStore.add({ WH_NO: tb_wh_no[i].VALUE, WH_NAME: tb_wh_no[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 240;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'datefield',
                    fieldLabel: '結案日期',
                    name: 'P0',
                    id: 'P0',
                    value: getDefaultValue(),
                    fieldCls: 'required',
                    allowBlank: false
                },
                {
                    xtype: 'datefield',
                    labelWidth: 70,
                    fieldLabel: '至',
                    labelSeparator: '',
                    name: 'P1',
                    id: 'P1',
                    value: getDefaultValue(),
                    fieldCls: 'required',
                    allowBlank: false,
                }
            ]
        },
        {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                {
                    xtype: 'combo',
                    store: Applywh_noStore,
                    fieldLabel: '申請庫別',
                    name: 'P2',
                    id: 'P2',
                    labelWidth: 65,
                    width: 235,
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    requiredFields: ['WH_NAME'],
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'combo',
                    store: WriteOffwh_noStore,
                    fieldLabel: '銷帳庫別',
                    name: 'P3',
                    id: 'P3',
                    labelWidth: 65,
                    width: 235,
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'WH_NAME',
                    valueField: 'WH_NO',
                    requiredFields: ['WH_NAME'],
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{WH_NO}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{WH_NO}</span><br/>&nbsp;<span style="color:blue">{WH_NAME}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true,
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                        select: function (combo, records, eOpts) {

                        }
                    },
                    padding: '0 4 0 4'
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    style: 'margin:0px 5px 0px 45px;',
                    handler: function () {
                        if (Ext.getCmp('P0').validate() && Ext.getCmp('P1').validate()) {
                            showReport();
                        }
                        else {
                            Ext.Msg.alert('訊息', '需填[結案日期]起迄才能查詢');
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    style: 'margin:0px 5px;',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]     //新增 修改功能畫面
        }],
    });

    function getDefaultValue() {
        tmp_Date = new Date();
        return tmp_Date;
    }

    function showReport() {
        if (!win) {
            var np = {
                p0: T1Query.getForm().findField('P0').getRawValue(),
                p1: T1Query.getForm().findField('P1').getRawValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
            };

            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p3=' + np.p3 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 20, viewport.height - 20);
        }
        win.show();
    }

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        },
        ]
    });
});