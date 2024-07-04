Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Name = "對帳單列印";

    var T1Rec = 0;
    var T1LastRec = null;
    var reportUrl = '', agenMemo = '';
    var agenExport = '../../../api/BG0009/AgenExcel';

    //物料分類Store
    var st_MatClass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0009/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    //來源代碼
    var st_SourceCode = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0009/GetSourcecodeCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //合約識別碼
    var st_Mcontid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0009/GetMcontidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    // 查詢欄位
    var mLabelWidth = 70;
    var mWidth = 230;
    var AgennoComboGet = '/api/BG0009/GetAgennoCombo';
    var T1FormAgenno = Ext.create('WEBAPP.form.AgenNoCombo', {
        name: 'P2',
        id: 'P2',
        fieldLabel: '廠商代碼',
        allowBlank: true,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        labelWidth: 70,
        width: 170,
        padding: '0 1 0 1',
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
    var T1FormAgenno_1 = Ext.create('WEBAPP.form.AgenNoCombo_1', {
        name: 'P2_1',
        id: 'P2_1',
        fieldLabel: '~',
        allowBlank: true,
        limit: 20,
        queryUrl: AgennoComboGet,
        storeAutoLoad: true,
        insertEmptyRow: true,
        labelWidth: 5,
        width: 105,
        padding: '0 1 0 1',
        listeners: {
            focus: function (field, event, eOpts) {
                T1Query.getForm().findField('P2_1').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });
    // 廠商統編 Combo
    var T1PhVendorCombo = Ext.create('WEBAPP.form.QueryCombo', {
        name: 'P7',
        id: 'P7',
        fieldLabel: '廠商統編',
        allowBlank: true,
        limit: 20,
        queryUrl: '/api/BG0009/GetPhVenderUniNoCombo',
        displayField: 'UNI_NO',
        valueField: 'UNI_NO',
        requiredFields: ['EASYNAME', 'AGEN_NAMEC'],
        tpl: new Ext.XTemplate(
            '<tpl for=".">',
            '<tpl if="VALUE==\'\'">',
            '<div class="x-boundlist-item" style="height:auto;">{UNI_NO}&nbsp;</div>',
            '<tpl else>',
            '<div class="x-boundlist-item" style="height:auto;border-bottom: 2px dashed #0a0;">' +
            '<span style="color:red">{UNI_NO}</span><br/>&nbsp;<span style="color:blue">{EASYNAME}</span></div>',
            '</tpl></tpl>', {
                formatText: function (text) {
                    return Ext.util.Format.htmlEncode(text);
                }
            }),
        width: 230,
        listeners: {
            onload: function (store, records, success) {
                console.log(records);
            },
            focus: function (field, event, eOpts) {
                Ext.getCmp('P7').setValue('');
                if (!field.isExpanded) {
                    setTimeout(function () {
                        field.expand();
                    }, 300);
                }
            }
        }
    });

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: false, // 若為true,容器內容超出容器大小時出現捲動條;若為false超出容器的部分會被裁切掉
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
                id: 'PanelP0',
                border: false,
                layout: 'hbox',
                bodyStyle: 'padding: 3px 5px;',
                items: [{
                    xtype: 'label',
                    text: '注意:本項目必需在月結之後才能列印，請輸入要對帳的民國年月共五碼',
                    style: { color: 'red' },
                    width: 490
                }, {
                    xtype: 'textfield',
                    fieldLabel: '對帳年月',
                    name: 'pDATA_YM',
                    id: 'pDATA_YM',
                    labelWidth: 80,
                    width: 240,
                    allowBlank: false, // 欄位為必填
                    fieldCls: 'required',
                    listeners: {
                        blur: function (field, eOpts) {
                            if (field.getValue() != '' && field.getValue() != null
                                && field.readOnly == false) {
                                T1Grid.getStore().removeAll();
                                chkDATA_YM(field.getValue());
                            }
                        }
                    }
                }]
            }]
        }, {
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [{
                xtype: 'combo',
                fieldLabel: '物料分類',
                store: st_MatClass,
                name: 'P0',
                id: 'P0',
                labelWidth: 70,
                width: 230,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                multiSelect: true
            }, {
                xtype: 'combo',
                fieldLabel: '買斷/寄庫',
                store: st_SourceCode,
                name: 'P1',
                id: 'P1',
                labelWidth: 70,
                width: 150,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '合約識別碼',
                store: st_Mcontid,
                name: 'P3',
                id: 'P3',
                labelWidth: 80,
                width: 160,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'textfield',
                fieldLabel: '合約案號',
                name: 'P4',
                id: 'P4',
                width: 200
            }]
        }, {
            xtype: 'panel',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px',
            items: [
                T1PhVendorCombo,
                {
                    xtype: 'textfield',
                    fieldLabel: '廠商名稱',
                    name: 'P5',
                    id: 'P5',
                    labelWidth: 80,
                    width: 240
                },
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            bodyStyle: 'padding: 3px 5px;',
            items: [
                T1FormAgenno,
                T1FormAgenno_1,
                 {
                    xtype: 'fieldcontainer',
                    fieldLabel: ' ',
                    defaultType: 'checkboxfield',
                    labelWidth: 8,
                    labelSeparator: '', //需有空格labelWidth才有用
                    width: 240,
                    items: [
                        {
                            boxLabel: '隱藏零庫存、未進出、無消耗之品項',
                            name: 'P6',
                            inputValue: '1',
                            id: 'checkbox1'
                        }]
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        msglabel('訊息區:');
                        if ((this.up('form').getForm().findField('pDATA_YM').getValue() == '' ||
                            this.up('form').getForm().findField('pDATA_YM').getValue() == null)) {
                            Ext.Msg.alert('提醒', '[對帳年月]不可空白');
                        } else {
                            T1Load();
                        }
                    }
                }]
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'MMCODE', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'DISC_CPRICE', type: 'string' },
            { name: 'P_INV_QTY', type: 'string' },
            { name: 'IN_QTY', type: 'string' },
            { name: 'REJ_OUTQTY', type: 'string' },
            { name: 'MIL_QTY', type: 'string' },
            { name: 'OUT_QTY', type: 'string' },
            { name: 'T_OUT_QTY', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'WRESQTY', type: 'string' },
            { name: 'TOT_AMT_3', type: 'string' },
            { name: 'EXTRA_DISC_AMOUNT', type: 'string' },
            { name: 'TOT_AMT_1', type: 'string' },
            { name: 'TOT_AMT_2', type: 'string' },
            { name: 'E_SOURCECODE', type: 'string' },
            { name: 'M_AGENNO', type: 'string' },
            { name: 'SPXFEE', type: 'string' },
            { name: 'M_NHIKEY', type: 'string' },
            { name: 'UNI_NO', type: 'string' },
            { name: 'NHI_PRICE', type: 'string' },
        ]
    });

    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 9999, // 每頁顯示筆數
        remoteSort: true,
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BG0009/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
            timeout: 180000
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    data_ym: T1Query.getForm().findField('pDATA_YM').getValue(),
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p2_1: T1Query.getForm().findField('P2_1').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue(),
                    p5: T1Query.getForm().findField('P5').getValue(),
                    p6: T1Query.getForm().findField('P6').getValue(),
                    p7: T1Query.getForm().findField('P7').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Store.load();
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
        buttons: [{
            itemId: 'btnSendMail', text: '寄送MAIL',
            handler: function () {
                var errMsg = '';
                if ((T1Query.getForm().findField('pDATA_YM').getValue() == '' || T1Query.getForm().findField('pDATA_YM').getValue() == null)) {
                    errMsg = "[對帳年月]不可空白";
                }
                if ((T1Query.getForm().findField('P2').getValue() == '' || T1Query.getForm().findField('P2').getValue() == null) &&
                    (T1Query.getForm().findField('P2_1').getValue() == '' || T1Query.getForm().findField('P2_1').getValue() == null)) {
                    errMsg = "[廠商代碼]不可空白";
                }
                if (errMsg != '') {
                    Ext.Msg.alert('提醒', errMsg);
                } else {
                    Ext.Ajax.request({
                        url: '/api/BG0009/GetPH_VENDER_Memo', // 截取附註
                        method: reqVal_p,
                        params: {
                            AGENNO: T1Query.getForm().findField('P2').getValue(),
                            AGENNO_1: T1Query.getForm().findField('P2_1').getValue()
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success == true) {
                                agenMemo = data.msg;
                                showWin();
                            }
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '截取附註發生例外錯誤');
                        }
                    }); // end of /api/BG0009/GetPH_VENDER_Memo
                }
            }
        }, {
            itemId: 'btnPrintAgen', text: '廠商支付明細表', disabled: true,
            handler: function () {
                if ((T1Query.getForm().findField('pDATA_YM').getValue() == '' ||
                    T1Query.getForm().findField('pDATA_YM').getValue() == null)) {
                    Ext.Msg.alert('提醒', '[對帳年月]不可空白');
                } else {
                    reportUrl = '/Report/B/BG0009.aspx?act=A';
                    showReport(reportUrl);
                }
            }
        }, {
            itemId: 'btnPrintAgen2', text: '匯出廠商支付明細表', disabled: true,
            handler: function () {
                if ((T1Query.getForm().findField('pDATA_YM').getValue() == '' ||
                    T1Query.getForm().findField('pDATA_YM').getValue() == null)) {
                    Ext.Msg.alert('提醒', '[對帳年月]不可空白');
                } else {
                    var np = {
                        pDATA_YM: T1Query.getForm().findField('pDATA_YM').getValue(),
                        p0: T1Query.getForm().findField('P0').getValue(),
                        p1: T1Query.getForm().findField('P1').getValue(),
                        p2: T1Query.getForm().findField('P2').getValue(),
                        p2_1: T1Query.getForm().findField('P2_1').getValue(),
                        p3: T1Query.getForm().findField('P3').getValue(),
                        p4: T1Query.getForm().findField('P4').getValue(),
                        p5: T1Query.getForm().findField('P5').getValue(),
                        p6: T1Query.getForm().findField('P6').getValue()
                    };

                    var p = new Array();
                    p.push({ name: 'pDATA_YM', value: T1Query.getForm().findField('pDATA_YM').getValue() });
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p2_1', value: T1Query.getForm().findField('P2_1').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'p5', value: T1Query.getForm().findField('P5').getValue() });
                    p.push({ name: 'p6', value: T1Query.getForm().findField('P6').getValue() });

                    PostForm(agenExport, p);
                    msglabel('訊息區:匯出完成');
                }
            }
        }, {
            itemId: 'btnPrintGroup', text: '廠商分組明細表', disabled: true,
            handler: function () {
                if ((T1Query.getForm().findField('pDATA_YM').getValue() == '' ||
                    T1Query.getForm().findField('pDATA_YM').getValue() == null)) {
                    Ext.Msg.alert('提醒', '[對帳年月]不可空白');
                } else {
                    reportUrl = '/Report/B/BG0009.aspx?act=G';
                    showReport(reportUrl);
                }
            }
        }, {
            itemId: 'btnPrintDetail', text: '印明細', disabled: true,
            handler: function () {
                if ((T1Query.getForm().findField('pDATA_YM').getValue() == '' ||
                    T1Query.getForm().findField('pDATA_YM').getValue() == null)) {
                    Ext.Msg.alert('提醒', '[對帳年月]不可空白');
                } else {
                    reportUrl = '/Report/B/BG0009.aspx?act=DL';
                    showReport(reportUrl);
                }
            }
        }, {
            itemId: 'btnPrintMail', text: '廠商貨款對帳單', disabled: true,
            handler: function () {
                if ((T1Query.getForm().findField('pDATA_YM').getValue() == '' ||
                    T1Query.getForm().findField('pDATA_YM').getValue() == null)) {
                    Ext.Msg.alert('提醒', '[對帳年月]不可空白');
                } else {
                    if ((T1Query.getForm().findField('P2').getValue() == '' ||
                        T1Query.getForm().findField('P2').getValue() == null)) {
                        Ext.Msg.alert('提醒', '[廠商代碼]不可空白');
                    } else {
                        reportUrl = '/Report/B/BG0009M.aspx?';
                        showReport(reportUrl);
                    }
                }
            }
            }, {
                itemId: 'btnNoteEdit', text: '修改附註', disabled: false,
                handler: function () {
                    msglabel('訊息區:');
                    editMemoWindow.show();
                    getMemo();
                }
            }]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
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
        columns: [{
            xtype: 'rownumberer',
            width: 40
        }, {
            text: "藥品代碼",
            dataIndex: 'MMCODE',
            width: 80
        }, {
            text: "品名",
            dataIndex: 'MMNAME_C',
            width: 160
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 60
        }, {
            text: "單價",
            dataIndex: 'M_CONTPRICE',
            width: 100
        }, {
            text: "優惠單價",
            dataIndex: 'DISC_CPRICE',
            width: 100
        }, {
            text: "上月結存",
            dataIndex: 'P_INV_QTY',
            width: 90
        }, {
            text: "本月進貨",
            dataIndex: 'IN_QTY',
            width: 90
        }, {
            text: "本月退貨",
            dataIndex: 'REJ_OUTQTY',
            width: 90
        }, {
            text: "本月軍用",
            dataIndex: 'MIL_QTY',
            width: 90
        }, {
            text: "本月民用",
            dataIndex: 'OUT_QTY',
            width: 90
        }, {
            text: "總支用",
            dataIndex: 'T_OUT_QTY',
            width: 90
        }, {
            text: "本月結存",
            dataIndex: 'INV_QTY',
            width: 90
        }, {
            text: "戰備存量",
            dataIndex: 'WRESQTY',
            width: 90
        }, {
            text: "聯標契約優惠",
            dataIndex: 'TOT_AMT_3',
            width: 100
        }, {
            text: "折讓金額",
            dataIndex: 'EXTRA_DISC_AMOUNT',
            width: 100
        }, {
            text: "應付總價",
            dataIndex: 'TOT_AMT_1',
            width: 100
        }, {
            text: "優惠應付總價",
            dataIndex: 'TOT_AMT_2',
            width: 100
        }, {
            text: "買/寄",
            dataIndex: 'E_SOURCECODE',
            width: 100
        }, {
            text: "廠商代碼",
            dataIndex: 'M_AGENNO',
            width: 100
        }, {
            text: "廠商統編",
            dataIndex: 'UNI_NO',
            width: 100
        }, {
            text: "特材代碼",
            dataIndex: 'SPXFEE',
            width: 100
        }, {
            text: "健保碼",
            dataIndex: 'M_NHIKEY',
            width: 100
        }, {
            text: "健保費",
            dataIndex: 'NHI_PRICE',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
            }
        }
    });

    function chkDATA_YM(parData_ym) {
        Ext.Ajax.request({
            url: '/api/BG0009/chkDATA_YM',
            method: reqVal_p,
            params: { data_ym: parData_ym },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'N') {
                        Ext.Msg.alert('訊息', '對帳年月不正確,請重新輸入!');
                        T1Tool.down('#btnPrintAgen').setDisabled(true);
                        T1Tool.down('#btnPrintAgen2').setDisabled(true);
                        T1Tool.down('#btnPrintGroup').setDisabled(true);
                        T1Tool.down('#btnPrintDetail').setDisabled(true);
                        T1Tool.down('#btnPrintMail').setDisabled(true);
                    } else {
                        T1Tool.down('#btnPrintAgen').setDisabled(false);
                        T1Tool.down('#btnPrintAgen2').setDisabled(false);
                        T1Tool.down('#btnPrintGroup').setDisabled(false);
                        T1Tool.down('#btnPrintDetail').setDisabled(false);
                        T1Tool.down('#btnPrintMail').setDisabled(false);
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function getMemo() {
        Ext.Ajax.request({
            url: '/api/BG0009/GetParamDMemo',
            method: reqVal_g,
            //params: { },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                   Ext.getCmp('editMemo_text').setValue(data.msg);
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function showReport(reportUrl) {
        if (!win) {
            var np = {
                pDATA_YM: T1Query.getForm().findField('pDATA_YM').getValue(),
                p0: T1Query.getForm().findField('P0').getValue(),
                p1: T1Query.getForm().findField('P1').getValue(),
                p2: T1Query.getForm().findField('P2').getValue(),
                p2_1: T1Query.getForm().findField('P2_1').getValue(),
                p3: T1Query.getForm().findField('P3').getValue(),
                p4: T1Query.getForm().findField('P4').getValue(),
                p5: T1Query.getForm().findField('P5').getValue(),
                p6: T1Query.getForm().findField('P6').getValue()
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '&pDATA_YM=' + np.pDATA_YM + '&p0=' + np.p0 + '&p1=' + np.p1 + '&p2=' + np.p2 + '&p2_1=' + np.p2_1 + '&p3=' + np.p3 + '&p4=' + np.p4 + '&p5=' + np.p5 + '&p6=' + np.p6 + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    var popForm = Ext.widget({
        xtype: 'form',
        layout: 'fit',
        frame: false,
        cls: 'T1b',
        title: '',
        bodyPadding: '5 5 0',
        fieldDefaults: {
            msgTarget: 'side',
            labelWidth: 40
        },
        autoScroll: true,
        items: [{
            xtype: 'textareafield',
            fieldLabel: '附註',
            name: 'NOTE',
            grow: true,
            anchor: '100%'
        }],
        buttons: [{
            itemId: 'popSubmit', text: '傳送', handler: function () {
                if (this.up('form').getForm().isValid()) {
                    Ext.MessageBox.confirm('寄送MAIL', '是否確定傳送?', function (btn, text) {
                        if (btn === 'yes') {
                            //step1:先將附註更新PH_VENDER的MEMO
                            Ext.Ajax.request({
                                url: '/api/BG0009/SavePH_VENDER_Memo',
                                method: reqVal_p,
                                params: {
                                    AGENNO: T1Query.getForm().findField('P2').getValue(),
                                    AGENNO_1: T1Query.getForm().findField('P2_1').getValue(),
                                    NOTE: popForm.getForm().findField('NOTE').getValue()
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        Ext.Ajax.request({
                                            url: '/api/BG0009/SendMail',
                                            method: reqVal_p,
                                            params: {
                                                pDATA_YM: T1Query.getForm().findField('pDATA_YM').getValue(),
                                                p2: T1Query.getForm().findField('P2').getValue(),
                                                p2_1: T1Query.getForm().findField('P2_1').getValue()
                                            },
                                            success: function (response) {
                                                var data = Ext.decode(response.responseText);
                                                if (data.success) {
                                                    Ext.MessageBox.alert('訊息', '寄送MAIL成功');
                                                }
                                                else
                                                    Ext.MessageBox.alert('錯誤', data.msg);
                                            },
                                            failure: function (response) {
                                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                            }
                                        });
                                        hideWin();
                                    }
                                    else
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    });
                }
            }
        }, {
            itemId: 'cancel', text: '取消', handler: hideWin
        }]
    });

    var win;
    var winActWidth = 700;
    var winActHeight = 400;
    if (!win) {
        win = Ext.widget('window', {
            title: '使用EMail傳送',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: popForm,
            listeners: {
                move: function (xwin, x, y, eOpts) {
                    xwin.setWidth((viewport.width - winActWidth > 0) ? winActWidth : viewport.width - 36);
                    xwin.setHeight((viewport.height - winActHeight > 0) ? winActHeight : viewport.height - 36);
                },
                resize: function (xwin, width, height) {
                    winActWidth = width;
                    winActHeight = height;
                }
            }
        });
    }
    function showWin() {
        if (win.hidden) {
            popForm.getForm().findField('NOTE').setValue(agenMemo);
            win.show();
        }
    }
    function hideWin() {
        if (!win.hidden) {
            win.hide();
        }
    }

    var editMemoWindow = Ext.create('Ext.window.Window', {
        id: 'editMemoWindow',
        renderTo: Ext.getBody(),
        items: [
            {
                xtype: 'panel',
                id: 'editMemo_Panel',
                border: false,
                layout: 'hbox',
                padding: '4 4 4 4',
                items: [
                    {
                        xtype: 'textareafield',
                        fieldLabel: '內容',
                        labelWidth: 80,
                        padding: '4 4 4 4',
                        id: 'editMemo_text',
                        name: 'editMemo_text',
                        enforceMaxLength: true,
                        maxLength: 300,
                        //readOnly: true,
                        width: "100%",
                        height: 250,
                    }
                ]
            }
        ],
        modal: true,
        width: "600px",
        height: 350,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        title: "修改附註",
        buttons: [
            {
                text: '儲存',
                handler: function () {
                    console.log('修改附註-儲存');
                    var data_value = Ext.getCmp('editMemo_text').getValue();
                    Ext.Ajax.request({
                        url: '/api/BG0009/SetParamDMemo',
                        method: reqVal_p,
                        params: {
                            MEMO: data_value
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success == true) {
                                editMemoWindow.hide();
                                msglabel('附註修改完成');
                            } else {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤-【附註】沒修改成功!!!');
                            }
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }
            , {
                text: '取消',
                handler: function () {
                    //console.log('信件內容維護-取消');
                    editMemoWindow.hide();
                }
            }
        ],
        listeners: {
            show: function (self, eOpts) {
                editMemoWindow.center();
                editMemoWindow.setWidth(600);
            }
        }
    });
    editMemoWindow.hide();

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [{
            itemId: 't1Grid',
            region: 'center',
            layout: 'fit',
            collapsible: false,
            title: '',
            border: false,
            items: [T1Grid]
        }]
    });
});