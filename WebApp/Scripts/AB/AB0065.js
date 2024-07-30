Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除

    var T1RecLength = 0;
    var T1LastRec = null;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });
    var TRCODEGet = '../../../api/AB0065/TRCODEGet'


    //交易碼
    var TRCODEQueryStore = Ext.create('Ext.data.Store', {
        fields: ['DATA_VALUE', 'DATA_DESC']
    });

    function setComboData() {
        //交易碼
        Ext.Ajax.request({
            url: TRCODEGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_tr = data.etts;
                    if (tb_tr.length > 0) {
                        for (var i = 0; i < tb_tr.length; i++) {
                            TRCODEQueryStore.add({ DATA_VALUE: tb_tr[i].DATA_VALUE, DATA_DESC: tb_tr[i].DATA_DESC });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();

    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P1',
        name: 'P1',
        fieldLabel: '藥品代碼',
        width: 300,
        labelAlign: 'left',
        labelWidth: 60,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0065/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    var T2QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P2',
        name: 'P2',
        fieldLabel: '~',
        labelAlign: 'right',
        width: 300,
        labelWidth: 20,
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0065/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數

        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
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
        layout: {
            type: 'vbox',
        },
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'combo',
                    store: TRCODEQueryStore,
                    fieldLabel: '交易碼',
                    labelWidth: 45,
                    width: 200,
                    name: 'TRCODE1',
                    id: 'TRCODE1',
                    queryMode: 'local',
                    anyMatch: true,
                    autoSelect: true,
                    displayField: 'DATA_VALUE',
                    valueField: 'DATA_VALUE',
                    requiredFields: ['DATA_DESC'],
                    fieldCls: 'required',
                    allowBlank: false, // 欄位為必填
                    tpl: new Ext.XTemplate(
                        '<tpl for=".">',
                        '<tpl if="VALUE==\'\'">',
                        '<div class="x-boundlist-item" style="height:auto;">{DATA_VALUE}&nbsp;</div>',
                        '<tpl else>',
                        '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
                        '<span style="color:red">{DATA_VALUE}</span><br/>&nbsp;<span style="color:blue">{DATA_DESC}</span></div>',
                        '</tpl></tpl>', {
                            formatText: function (text) {
                                return Ext.util.Format.htmlEncode(text);
                            }
                        }),
                    listeners: {
                        beforequery: function (record) {
                            record.query = new RegExp(record.query, 'i');
                            record.forceAll = true;
                        },
                    },
                },
                {
                    xtype: 'datefield',
                    fieldLabel: '日期範圍',
                    name: 'D0',
                    id: 'D0',
                    vtype: 'dateRange',
                    dateRange: { end: 'D1' },
                    fieldCls: 'required',
                    allowBlank: false, // 欄位為必填
                    value: getDefaultValue(),
                    labelWidth: 60,
                    width: 160
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'D1',
                    id: 'D1',
                    fieldCls: 'required',
                    allowBlank: false, // 欄位為必填
                    value: getDefaultValue(),
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'D0' },
                    labelWidth: 7,
                    width: 107
                }, {
                    xtype: 'textfield',
                    fieldLabel: '病歷號',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: 60,
                    width: 160
                }
            ]

        }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    T1QuryMMCode,
                    T2QuryMMCode,
                    {
                        xtype: 'textfield',
                        fieldLabel: '藥品名稱',
                        name: 'P3',
                        id: 'P3',
                        labelWidth: 60,
                        width: 240
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '~',
                        labelWidth: 7,
                        width: 107,
                        name: 'P4',
                        id: 'P4',
                        labelWidth: 7,
                        width: 180
                    },
                    {
                        xtype: 'button',
                        text: '列印',
                        handler: function () {
                            if (T1Query.getForm().findField('TRCODE1').getValue() == "" || T1Query.getForm().findField('TRCODE1').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填交易碼才能查詢');

                            } else if (T1Query.getForm().findField('D0').getValue() == "" || T1Query.getForm().findField('D0').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填日期範圍才能查詢');

                            } else if (T1Query.getForm().findField('D1').getValue() == "" || T1Query.getForm().findField('D1').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填日期範圍才能查詢');

                            } else {
                                reportUrl = '/Report/A/AB0065.aspx';
                                showReport();
                            }


                        }
                    },
                    {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('TRCODE1').focus(); // 進入畫面時輸入游標預設在TRCODE1
                            msglabel('訊息區:');
                        }
                    }
                ],

            }]
    });

    function getDefaultValue() {
        var yyyy = 0;
        var m = 0;

        yyyy = new Date().getFullYear() - 1911;
        m = new Date().getMonth() + 1;     

        var d = 0;
        d = new Date().getDate();

        var mm = m >= 10 ? m.toString() : "0" + m.toString();
        var dd = d >= 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;
    }

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?D0=' + T1Query.getForm().findField('D0').rawValue
                    + '&D1=' + T1Query.getForm().findField('D1').rawValue
                    + '&P0=' + T1Query.getForm().findField('P0').getValue()
                    + '&P1=' + T1Query.getForm().findField('P1').getValue()
                    + '&P2=' + T1Query.getForm().findField('P2').getValue()
                    + '&P3=' + T1Query.getForm().findField('P3').getValue()
                    + '&P4=' + T1Query.getForm().findField('P4').getValue()
                    + '&TRCODE1=' + T1Query.getForm().findField('TRCODE1').getValue()
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    var T1Store = Ext.create('WEBAPP.store.MiUnitcodeAA0119', { // 目前在AB0065沒用到
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store, //資料load進來
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