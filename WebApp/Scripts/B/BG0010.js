Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});
Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1Name = "支付廠商眀細表列印";
    var UrlReport = '/Report/B/BG0010.aspx';
    var  data_ym = "";

    var st_matclass = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'COMBITEM']
    });

    function setComboData() {
        Ext.Ajax.request({
            url: '/api/BG0010/GetYm',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    data_ym = data.afrs;
                    T1Query.getForm().findField("P0").setValue(data.afrs);
                }
            }
        });

        //P2 藥材類別
        Ext.Ajax.request({
            url: '/api/BG0010/GetMatClassCombo',
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var matclass = data.etts;
                    if (matclass.length > 0) {
                        for (var i = 0; i < matclass.length; i++) {
                            st_matclass.add({ VALUE: matclass[i].VALUE, COMBITEM: matclass[i].COMBITEM });
                        }
                    }
                }
            }
        });
    }
    setComboData();

    var T1QueryAgenNoCombo = Ext.create('WEBAPP.form.AgenNoCombo_2', {
        name: 'P2',
        id: 'P2',
        fieldLabel: '檢視支付廠商區間',
        labelAlign: 'right',
        allowBlank: true,
        labelWidth: 135,
        width: 305,
        limit: 10,//限制一次最多顯示10筆
        queryUrl: '/api/BG0010/GetAgenNoCombo',//指定查詢的Controller路徑
        storeAutoLoad: true,
        // insertEmptyRow: true,
        //     extraFields: ['AGEN_NO', 'AGEN_NAMEC'],//查詢完會回傳的欄位
        listeners: {
            focus: function (field, event, eOpts) {
                T1Query.getForm().findField('P2').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });

    var T1QueryAgenNoCombo_1 = Ext.create('WEBAPP.form.AgenNoCombo_2', {
        name: 'P3',
        id: 'P3',
        fieldLabel: '至',
        labelAlign: 'right',
        allowBlank: true,
        labelWidth: 8,
        width: 178,
        limit: 10,//限制一次最多顯示10筆
        queryUrl: '/api/BG0010/GetAgenNoCombo_1',//指定查詢的Controller路徑
        //  extraFields: ['AGEN_NO', 'AGEN_NAMEC'],//查詢完會回傳的欄位
        listeners: {
            focus: function (field, event, eOpts) {
                T1Query.getForm().findField('P3').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        //autoScroll: true, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [{
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                layout: 'hbox',
                bodyStyle: 'padding: 3px 5px;',
                items: [{
                    xtype: 'monthfield',
                    fieldLabel: '輸入支付廠商應付款列表年月',
                    name: 'P0',
                    id: 'P0',
                    labelWidth: 200,
                    width: 300,
                    fieldCls: 'required',
                    value: new Date(),
                    allowBlank: false
                }, {
                    xtype: 'combo',
                    fieldLabel: '物料類別',
                    name: 'P1',
                    id: 'P1',
                    labelWidth: 70,
                    width: 260,
                    emptyText: '全部',
                    store: st_matclass,
                    queryMode: 'local',
                    displayField: 'COMBITEM',
                    valueField: 'VALUE',
                    multiSelect: true
                }]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                layout: 'hbox',
                items: [
                    T1QueryAgenNoCombo,
                    T1QueryAgenNoCombo_1,
                    {
                        xtype: 'button',
                        text: '重新計算',
                        id: 'T1btn1',
                        handler: function () {
                            if (T1Query.getForm().findField("P0").getValue() == null || T1Query.getForm().findField("P0").getValue() == "") {
                                Ext.Msg.alert('提醒', '年月不可空白');
                            } else {
                                ReCalucate(T1Query.getForm().findField("P0").rawValue);
                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '查詢',
                        handler: function () {
                            if ((T1Query.getForm().findField("P0").getValue() == null) || (T1Query.getForm().findField("P0").getValue() == "")) {
                                Ext.Msg.alert('提醒', '年月不可空白');
                            } else {
                                T1Load();
                            }
                        }
                    }, {
                        xtype: 'button',
                        text: '清除',
                        handler: function () {
                            var f = this.up('form').getForm();
                            f.reset();
                            f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        }
                    }
                ]
            }, {
                xtype: 'panel',
                id: 'PanelP3',
                border: false,
                layout: 'hbox',
                items: [{
                    xtype: 'displayfield',
                    fieldLabel: '<font color="blue">月結起迄日期</font>',
                    name: 'P_MIMNSET',
                    id: 'P_MIMNSET',
                    style: { color: 'blue' },
                    labelWidth: 110,
                    width: 300
                }]
            }]
        }]
    });

    var mLabelWidth = 120;
    var mWidth = 250;
    var T2Form = Ext.create('Ext.form.Panel', {
        xtype: 'form',
        bodyStyle: 'padding:5px 5px 0',
        layout: {
            type: 'table',
            columns: 4,
            border: true,
            bodyBorder: true,
            tdAttrs: { width: '25%' }
        },
        bodyPadding: '5 5 0 0',
        autoScroll: true,
        frame: false,
        defaults: {
            labelAlign: 'right',
            readOnly: true,
            labelWidth: mLabelWidth,
            width: mWidth,
            padding: '4 0 4 0',
            msgTarget: 'side'
        },
        defaultType: 'textfield',
        items: [

            { fieldLabel: '軍應付總金額', name: 'PAYARMY', readOnly: true, width: mWidth },
            { fieldLabel: '軍買斷總金額', name: 'BUYARMY', readOnly: true, width: mWidth },
            { fieldLabel: '軍寄庫總金額', name: 'TMPARMY', readOnly: true, width: mWidth },
            { fieldLabel: '聯標契約優惠', name: 'MGTFEE', readOnly: true, width: mWidth },

            { fieldLabel: '民應付總金額', name: 'PAYMASS', readOnly: true, width: mWidth },
            { fieldLabel: '民買斷總金額', name: 'BUYMASS', readOnly: true, width: mWidth },
            { fieldLabel: '民寄庫總金額', name: 'TMPMASS', readOnly: true, width: mWidth },
            { fieldLabel: '折讓總金額', name: 'DISAMOUNT', readOnly: true, width: mWidth }

        ]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'AGEN_NO', type: 'string' },
            { name: 'AGEN_NAMEC', type: 'string' },
            { name: 'PAYARMY', type: 'string' },
            { name: 'BUYARMY', type: 'string' },
            { name: 'TMPARMY', type: 'string' },
            { name: 'PAYMASS', type: 'string' },
            { name: 'BUYMASS', type: 'string' },
            { name: 'TMPMASS', type: 'string' },
            { name: 'MGTFEE', type: 'string' },
            { name: 'DISAMOUNT', type: 'string' },
            { name: 'MASSMOD', type: 'string' },
            { name: 'RCMOD', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name:'MORE_DISC_AMOUNT',type:'string'}
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'AGEN_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0010/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0~P4的值代入參數
                var np = {
                    p0: T1Query.getForm().findField('P0').rawValue,
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            },
            load: function (store, records, successful, eOpts) {
                if (successful) {
                    Ext.Ajax.request({
                        url: '/api/BG0010/SumAll',
                        params: {
                            p0: T1Query.getForm().findField('P0').rawValue,
                            p1: T1Query.getForm().findField('P1').getValue(),
                            p2: T1Query.getForm().findField('P2').getValue(),
                            p3: T1Query.getForm().findField('P3').getValue()
                        },
                        method: reqVal_p,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                T2Form.getForm().findField('PAYARMY').setValue(data.etts[0]['PAYARMY']);
                                T2Form.getForm().findField('BUYARMY').setValue(data.etts[0]['BUYARMY']);
                                T2Form.getForm().findField('TMPARMY').setValue(data.etts[0]['TMPARMY']);
                                T2Form.getForm().findField('MGTFEE').setValue(data.etts[0]['MGTFEE']);
                                T2Form.getForm().findField('PAYMASS').setValue(data.etts[0]['PAYMASS']);
                                T2Form.getForm().findField('BUYMASS').setValue(data.etts[0]['BUYMASS']);
                                T2Form.getForm().findField('TMPMASS').setValue(data.etts[0]['TMPMASS']);
                                T2Form.getForm().findField('DISAMOUNT').setValue(data.etts[0]['DISAMOUNT']);

                                Ext.Ajax.request({
                                    url: '/api/BG0010/GetMiMnset',
                                    method: reqVal_p,
                                    params: {
                                        p0: T1Query.getForm().findField('P0').rawValue
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            if (data.etts) {
                                                var p_mimnset = '<font color="blue">' + data.etts[0] + '</font>';
                                                T1Query.getForm().findField('P_MIMNSET').setValue(p_mimnset);
                                            }
                                        }
                                    }
                                });
                            }
                        }
                    });

                    var dataCount = store.getCount();
                    if (dataCount > 0) {
                        T1Tool.down('#print1').setDisabled(false);
                        T1Tool.down('#print2').setDisabled(false);
                        T1Tool.down('#print3').setDisabled(false);
                        T1Tool.down('#print4').setDisabled(false);
                        T1Tool.down('#btnUpdate').setDisabled(false);
                        T1Tool.down('#btnExcel').setDisabled(false);
                        T1Tool.down('#btnExcel2').setDisabled(false);
                    }
                    else {
                        T1Tool.down('#print1').setDisabled(true);
                        T1Tool.down('#print2').setDisabled(true);
                        T1Tool.down('#print3').setDisabled(true);
                        T1Tool.down('#print4').setDisabled(true);
                        T1Tool.down('#btnUpdate').setDisabled(true);
                        T1Tool.down('#btnExcel').setDisabled(true);
                        T1Tool.down('#btnExcel2').setDisabled(true);
                    }
                }
            }
        }
    });

    function T1Load() {
        T1Tool.moveFirst();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            itemId: 'print1', text: '支付明細列印', disabled: true,
            handler: function () {
                showReport(1);
            }
        }, {
            itemId: 'print2', text: '實付列印', disabled: true,
            handler: function () {
                showReport(2);
            }
        }, {
            itemId: 'print3', text: '聯標契約優惠列印', disabled: true,
            handler: function () {
                showReport(3);
            }
        }, {
            itemId: 'print4', text: '應付列印', disabled: true,
            handler: function () {
                showReport(4);
            }
        }, {
            text: '更新',
            id: 'btnUpdate',
            name: 'btnUpdate',
            disabled: true,
            handler: function () {
                var tempData = T1Grid.getStore().data.items;
                var data = [];
                for (var i = 0; i < tempData.length; i++) {
                    if (tempData[i].dirty) {
                        data.push(tempData[i].data);
                    }
                }
                var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                myMask.show();
                Ext.Ajax.request({
                    url: '/api/BG0010/Update',
                    method: reqVal_p,
                    contentType: "application/json",
                    params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                    success: function (response) {
                        myMask.hide();
                        var data = Ext.decode(response.responseText);
                        if (data.success == true) {
                            msglabel('訊息區:資料修改成功');
                            T1Tool.moveFirst();
                        } else {
                            Ext.MessageBox.alert('錯誤', data.msg);
                        }
                    },
                    failure: function (response, action) {
                        myMask.hide();
                        Ext.Msg.alert('失敗', 'Ajax communication failed');
                    }
                });
            }
        },
        {
            id: 'btnExcel',
            name: 'btnExcel',
            disabled: true,
            text: '匯出', handler: function () {
                var p = new Array();

                var p0 = T1Query.getForm().findField('P0').rawValue;
                var p1 = T1Query.getForm().findField('P1').getValue();
                var p2 = T1Query.getForm().findField('P2').getValue();
                var p3 = T1Query.getForm().findField('P3').getValue();

                p.push({ name: 'p0', value: p0 });
                p.push({ name: 'p1', value: p1 });
                p.push({ name: 'p2', value: p2 });
                p.push({ name: 'p3', value: p3 });
                PostForm('../../../api/BG0010/Excel', p);
                msglabel('訊息區:匯出完成');
            }
        },
        {
            id: 'btnExcel2',
            name: 'btnExcel2',
            disabled: true,
            text: '支付明細匯出', handler: function () {
                var p = new Array();

                var p0 = T1Query.getForm().findField('P0').rawValue;
                var p1 = T1Query.getForm().findField('P1').getValue();
                var p2 = T1Query.getForm().findField('P2').getValue();
                var p3 = T1Query.getForm().findField('P3').getValue();

                p.push({ name: 'p0', value: p0 });
                p.push({ name: 'p1', value: p1 });
                p.push({ name: 'p2', value: p2 });
                p.push({ name: 'p3', value: p3 });
                PostForm('../../../api/BG0010/Excel2', p);
                msglabel('訊息區:匯出完成');
            }
        }]
    });

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T1Store,
        plain: true,
        //loadingText: '處理中...',
        //loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: "廠商代碼", dataIndex: 'AGEN_NO', width: 100 },
            { text: "廠商名稱", dataIndex: 'AGEN_NAMEC', width: 100, style: 'text-align:left', align: 'center' },
            { text: "軍應付總金額", dataIndex: 'PAYARMY', width: 110, style: 'text-align:left', align: 'right' },
            { text: "軍買斷總金額", dataIndex: 'BUYARMY', width: 110, style: 'text-align:left', align: 'right' },
            { text: "軍寄庫金額", dataIndex: 'TMPARMY', width: 100, style: 'text-align:left', align: 'right' },
            { text: "民應付總金額", dataIndex: 'PAYMASS', width: 110, style: 'text-align:left', align: 'right' },
            { text: "民買斷總金額", dataIndex: 'BUYMASS', width: 110, style: 'text-align:left', align: 'right' },
            { text: "民寄庫金額", dataIndex: 'TMPMASS', width: 100, style: 'text-align:left', align: 'right' },
            { text: "聯標契約優惠", dataIndex: 'MGTFEE', width: 110, style: 'text-align:left', align: 'right' },
            { text: "折讓金額", dataIndex: 'DISAMOUNT', width: 100, style: 'text-align:left', align: 'right' },
            { text: "民應付調整金額", dataIndex: 'MASSMOD', width: 140, style: 'text-align:left', align: 'right' },
            {
                text: "<font color=red>聯標契約優惠調整金額</font>", dataIndex: 'RCMOD', width: 170, style: 'text-align:left', align: 'right',
                editor: {
                    xtype: 'textfield',
                    regexText: '只能輸入數字',
                    regex: /^(\-|\+)?\d+?$/ // 用正規表示式限制可輸入內容
                }
            },
            { text: "物料類別", dataIndex: 'MAT_CLASS', width: 90, style: 'text-align:left', align: 'left' },
            { text: "統一編號", dataIndex: 'UNI_NO', width: 90, style: 'text-align:left', align: 'center' },
            { text: "優惠金額", dataIndex: 'MORE_DISC_AMOUNT', width: 100, style: 'text-align:left', align: 'right' },
            {
                header: "",
                flex: 1
            }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
            })
        ]
    });

    function showReport(mode) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + UrlReport
                + '?rdlc=' + mode
                + '&P0=' + T1Query.getForm().findField('P0').rawValue
                + '&P1=' + T1Query.getForm().findField('P1').getValue()
                + '&P2=' + T1Query.getForm().findField('P2').rawValue
                + '&P3=' + T1Query.getForm().findField('P3').rawValue
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

    function ReCalucate(parSet_ym) {
        Ext.Ajax.request({
            url: '/api/BG0010/ReCalulate',
            method: reqVal_p,
            params: { set_ym: parSet_ym },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T1Load();
                }
                else
                    Ext.MessageBox.alert('錯誤', data.msg);
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
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
        items: [
            {
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                split: true,
                //  height: '80%',
                items: [T1Grid]
            },
            {
                region: 'south',
                layout: 'fit',
                collapsible: false,
                title: '',
                height: '10%',
                split: true,
                items: [T2Form]
            }
        ]
    });
});