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
    var NRCODEGet = '../../../api/AB0082/NRCODEGet'
    var WHNOGet = '../../../api/AB0082/WHNOGet'


    //病房
    var NRCODEQueryStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });
    //庫房
    var WHNOQueryStore = Ext.create('Ext.data.Store', {
        fields: ['WH_NO', 'WH_NAME']
    });

    function setComboData() {

        //病房
        Ext.Ajax.request({
            url: NRCODEGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_whno = data.etts;
                    if (tb_whno.length > 0) {
                        for (var i = 0; i < tb_whno.length; i++) {
                            NRCODEQueryStore.add({ WH_NO: tb_whno[i].WH_NO, WH_NAME: tb_whno[i].WH_NAME });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });

       //庫房
        Ext.Ajax.request({
            url: WHNOGet,
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_whno = data.etts;
                    if (tb_whno.length > 0) {
                        for (var i = 0; i < tb_whno.length; i++) {
                            WHNOQueryStore.add({ WH_NO: tb_whno[i].WH_NO, WH_NAME: tb_whno[i].WH_NAME });
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
                    xtype: 'datefield',
                    fieldLabel: '查詢日期',
                    name: 'D0',
                    id: 'D0',
                    vtype: 'dateRange',
                    dateRange: { end: 'D2' },
                    labelWidth: 60,
                    width: 160
                }, {
                    xtype: 'timefield',
                    name: 'D1',
                    id: 'D1',
                    reference: 'timeField',
                    format: 'H:i',
                    maxValue: '24:00',
                    increment: 5,
                    width: 100
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'D2',
                    id: 'D2',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'D0' },
                    labelWidth: 7,
                    width: 107
                }, {
                    xtype: 'timefield',
                    name: 'D3',
                    id: 'D3',
                    reference: 'timeField',
                    format: 'H:i',
                    maxValue: '24:00',
                    increment: 5,
                    width: 100
                }, {
                    xtype: 'datefield',
                    fieldLabel: '退藥日期',
                    name: 'D4',
                    id: 'D4',
                    vtype: 'dateRange',
                    dateRange: { end: 'D6' },
                    width: 180
                }, {
                    xtype: 'timefield',
                    name: 'D5',
                    id: 'D5',
                    reference: 'timeField',
                    format: 'H:i',
                    maxValue: '24:00',
                    increment: 5,
                    width: 100
                }, {
                    xtype: 'datefield',
                    fieldLabel: '至',
                    labelWidth: '10px',
                    name: 'D6',
                    id: 'D6',
                    labelSeparator: '',
                    vtype: 'dateRange',
                    dateRange: { begin: 'D4' },
                    labelWidth: 7,
                    width: 107
                }, {
                    xtype: 'timefield',
                    name: 'D7',
                    id: 'D7',
                    reference: 'timeField',
                    format: 'H:i',
                    maxValue: '24:00',
                    increment: 5,
                    width: 100
                }
            ]

        }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'combo',
                        store: NRCODEQueryStore,
                        fieldLabel: '病房',
                        labelWidth: 35,
                        width: 135,
                        name: 'NRCODE1',
                        id: 'NRCODE1',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        displayField: 'WH_NO',
                        valueField: 'WH_NO',
                        requiredFields: ['WH_NAME'],
                        fieldCls: 'required',
                        allowBlank: false, // 欄位為必填
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
                        listeners: {
                            beforequery: function (record) {
                                record.query = new RegExp(record.query, 'i');
                                record.forceAll = true;
                            },
                        },
                    }, {
                        xtype: 'combo',
                        store: NRCODEQueryStore,
                        fieldLabel: '~',
                        labelSeparator: '',
                        labelWidth: 7,
                        width: 107,
                        name: 'NRCODE2',
                        id: 'NRCODE2',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        displayField: 'WH_NO',
                        valueField: 'WH_NO',
                        requiredFields: ['WH_NAME'],
                        fieldCls: 'required',
                        allowBlank: false, // 欄位為必填
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
                        listeners: {
                            beforequery: function (record) {
                                record.query = new RegExp(record.query, 'i');
                                record.forceAll = true;
                            },
                        },
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '病歷號',
                        name: 'P0',
                        id: 'P0',
                        labelWidth: 60,
                        width: 160
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '~',
                        labelSeparator: '',
                        name: 'P1',
                        id: 'P1',
                        labelWidth: 7,
                        width: 107
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '病床號',
                        name: 'P2',
                        id: 'P2',
                        labelWidth: 60,
                        width: 160
                    }, {
                        xtype: 'textfield',
                        fieldLabel: '~',
                        labelSeparator: '',
                        labelWidth: 7,
                        width: 107,
                        name: 'P3',
                        id: 'P3',
                        labelWidth: 7,
                        width: 107
                    }, {
                        xtype: 'combo',
                        store: WHNOQueryStore,
                        fieldLabel: '庫別',
                        labelWidth: 50,
                        width: 160,
                        name: 'WH_NO',
                        id: 'WH_NO',
                        queryMode: 'local',
                        anyMatch: true,
                        autoSelect: true,
                        displayField: 'WH_NO',
                        valueField: 'WH_NO',
                        requiredFields: ['WH_NAME'],
                        fieldCls: 'required',
                        allowBlank: false, // 欄位為必填
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
                        listeners: {
                            beforequery: function (record) {
                                record.query = new RegExp(record.query, 'i');
                                record.forceAll = true;
                            },
                        },
                    }, {
                        xtype: 'button',
                        text: '病房總表列印',
                        handler: function () {
                            if (T1Query.getForm().findField('NRCODE1').getValue() == "" || T1Query.getForm().findField('NRCODE1').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填病房才能查詢');

                            } else if (T1Query.getForm().findField('NRCODE2').getValue() == "" || T1Query.getForm().findField('NRCODE2').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填病房才能查詢');

                            } else if (T1Query.getForm().findField('WH_NO').getValue() == "" || T1Query.getForm().findField('WH_NO').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填庫別才能查詢');

                            } else {
                                reportUrl = '/Report/A/AB0082.aspx';
                                //showReport();
                                showReportf();
                            }


                        }
                    }, {
                        xtype: 'button',
                        text: '病患清單列印',
                        handler: function () {
                            if (T1Query.getForm().findField('NRCODE1').getValue() == "" || T1Query.getForm().findField('NRCODE1').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填病房才能查詢');

                            } else if (T1Query.getForm().findField('NRCODE2').getValue() == "" || T1Query.getForm().findField('NRCODE2').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填病房才能查詢');

                            } else if (T1Query.getForm().findField('WH_NO').getValue() == "" || T1Query.getForm().findField('WH_NO').getValue() == null) {
                                Ext.Msg.alert('訊息', '需填庫別才能查詢');

                            } else {
                                reportUrl = '/Report/A/AB0082List.aspx';
                                //showReport();
                                showReportf();
                            }


                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('D0').focus(); // 進入畫面時輸入游標預設在P0
                            msglabel('訊息區:');

                            Ext.getDom('mainContent').src = '';
                        }
                    }
                ],

            }]
    });

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
                    + '&D2=' + T1Query.getForm().findField('D2').rawValue
                    + '&D3=' + T1Query.getForm().findField('D3').rawValue
                    + '&D4=' + T1Query.getForm().findField('D4').rawValue
                    + '&D5=' + T1Query.getForm().findField('D5').rawValue
                    + '&D6=' + T1Query.getForm().findField('D6').rawValue
                    + '&D7=' + T1Query.getForm().findField('D7').rawValue
                    + '&NRCODE1=' + T1Query.getForm().findField('NRCODE1').getValue()
                    + '&NRCODE2=' + T1Query.getForm().findField('NRCODE2').getValue()
                    + '&P0=' + T1Query.getForm().findField('P0').getValue()
                    + '&P1=' + T1Query.getForm().findField('P1').getValue()
                    + '&P2=' + T1Query.getForm().findField('P2').getValue()
                    + '&P3=' + T1Query.getForm().findField('P3').getValue()
                    + '&WH_NO=' + T1Query.getForm().findField('WH_NO').getValue()
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

    var T1Store = Ext.create('WEBAPP.store.MiUnitcodeAA0119', { // 定義於/Scripts/app/store/MiUnitcodeAA0119.js
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
    //var T1Grid = Ext.create('Ext.grid.Panel', {
    //    store: T1Store, //資料load進來
    //    plain: true,
    //    loadingText: '處理中...',
    //    loadMask: true,
    //    cls: 'T1',
    //    dockedItems: [{
    //        dock: 'top',
    //        xtype: 'toolbar',
    //        layout: 'fit',
    //        items: [T1Query]     //新增 修改功能畫面
    //    }],
    //});

    //var viewport = Ext.create('Ext.Viewport', {
    //    renderTo: body,
    //    layout: {
    //        type: 'border',
    //        padding: 0
    //    },
    //    defaults: {
    //        split: true
    //    },
    //    items: [{
    //        itemId: 't1Grid',
    //        region: 'center',
    //        layout: 'fit',
    //        collapsible: false,
    //        title: '',
    //        border: false,
    //        items: [T1Grid]
    //    },
    //    ]
    //});

    var form1 = Ext.create('Ext.form.Panel', {
        plain: true,
        resizeTabs: true,
        layout: 'fit',
        border: false,
        defaults: {
            autoScroll: true
        },
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }
        ]

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
            itemId: 'tabReport',
            region: 'north',
            frame: false,
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [form1]
        }, {
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

    function showReportf() {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        var qstring = '?D0=' + T1Query.getForm().findField('D0').rawValue
            + '&D1=' + T1Query.getForm().findField('D1').rawValue
            + '&D2=' + T1Query.getForm().findField('D2').rawValue
            + '&D3=' + T1Query.getForm().findField('D3').rawValue
            + '&D4=' + T1Query.getForm().findField('D4').rawValue
            + '&D5=' + T1Query.getForm().findField('D5').rawValue
            + '&D6=' + T1Query.getForm().findField('D6').rawValue
            + '&D7=' + T1Query.getForm().findField('D7').rawValue
            + '&NRCODE1=' + T1Query.getForm().findField('NRCODE1').getValue()
            + '&NRCODE2=' + T1Query.getForm().findField('NRCODE2').getValue()
            + '&P0=' + T1Query.getForm().findField('P0').getValue()
            + '&P1=' + T1Query.getForm().findField('P1').getValue()
            + '&P2=' + T1Query.getForm().findField('P2').getValue()
            + '&P3=' + T1Query.getForm().findField('P3').getValue()
            + '&WH_NO=' + T1Query.getForm().findField('WH_NO').getValue();

        Ext.getDom('mainContent').src = reportUrl + qstring;

        $('iframe#mainContent').load(function () {
            myMask.hide();
        });

    }
});