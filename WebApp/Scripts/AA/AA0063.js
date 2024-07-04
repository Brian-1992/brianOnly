Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    // var T1Get = '/api/AA0063/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "核撥明細表";

    var T1Rec = 0;
    var T1LastRec = null;
    var reportUrl = '/Report/A/AA0063.aspx';
    var T1GetExcel = '../../../api/AA0063/Excel';
    var GetCLSNAME = '../../../api/AA0063/GetCLSNAME';
    var GetFLOWID = '../../../api/AA0063/GetFLOWID';
    //var GetXCATEGORY = '../../../api/AA0063/GetXCATEGORY';
    var GetWh_no = '../../../api/AA0063/GetWh_no';

    var tempCLS;
    var parCLS = '';
    var printCLS = '';
    var exportCLS = '';

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var CLSNAMEStore = Ext.create('Ext.data.Store', {  //物料分類的store
        fields: ['VALUE', 'TEXT']
    });

    var FLOWIDStore = Ext.create('Ext.data.Store', {  //物料分類的store
        fields: ['VALUE', 'TEXT']
    });

    var getTodayDate = function () {
        var y = (new Date().getFullYear() - 1911).toString();
        var m = (new Date().getMonth() + 1).toString();
        var d = (new Date().getDate()).toString();
        m = m.length > 1 ? m : "0" + m;
        d = d.length > 1 ? d : "0" + d;
        return y + m + d;
    }

    function SetCLSNAME() { //建立物料分類的下拉式選單
        Ext.Ajax.request({
            url: GetCLSNAME,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var clsnames = data.etts;
                    tempCLS = clsnames;
                    if (clsnames.length > 0) {
                        CLSNAMEStore.add({ VALUE: '', TEXT: '全部' });
                        for (var i = 0; i < clsnames.length; i++) {
                            CLSNAMEStore.add({ VALUE: clsnames[i].VALUE, TEXT: clsnames[i].TEXT });
                        }
                    }
                    T1Query.getForm().findField("P0").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    function SetFLOWID() { //建立申請單狀態的下拉式選單
        Ext.Ajax.request({
            url: GetFLOWID,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var flowids = data.etts;
                    if (flowids.length > 0) {
                        FLOWIDStore.add({ VALUE: '', TEXT: '全部' });
                        FLOWIDStore.add({ VALUE: '1', TEXT: '1-申請中' });
                        FLOWIDStore.add({ VALUE: '2', TEXT: '2-核撥中' });
                        FLOWIDStore.add({ VALUE: '3', TEXT: '3-揀料中' });
                        FLOWIDStore.add({ VALUE: '4', TEXT: '4-點收中' });
                        FLOWIDStore.add({ VALUE: '5', TEXT: '5-已點收' });
                        FLOWIDStore.add({ VALUE: '51', TEXT: '51-已點收確認' });
                        FLOWIDStore.add({ VALUE: '6', TEXT: '6-已核撥確認' });
                    }
                    T1Query.getForm().findField("P4").setValue("");
                }
            },
            failure: function (response, options) {

            }
        });
    }

    //搜尋院內碼
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        id: 'P1',
        fieldLabel: '院內碼',
        allowBlank: true,
        labelWidth: 55,
        width: 185,

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T1Query.getForm();
            return {
                MMCODE: f.findField("P1").getValue(),
                MAT_CLASS: f.findField("P0").getValue()
            };
        },

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0063/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T1Query.getForm();
                if (r.get('P1') !== '') {
                    f.findField("P1").setValue(r.get('MMCODE'));
                }
            }
        }
    });

    function setWh_no() {
        Ext.Ajax.request({
            url: GetWh_no,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_no = data.etts;
                    if (wh_no.length > 0) {
                        T1Query.getForm().findField('WH_NO').setValue(wh_no[0].TEXT);
                        WHNO_MM1 = wh_no[0].VALUE;
                        getWhName = wh_no[0].TEXT;
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setWh_no();

    // 查詢欄位
    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        //frame: false,
        border: false,
        autoScroll: true,
        //bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 70,
            width: 180
        },
        items: [
            {
                xtype: 'panel',
                id: 'PanelP1',
                border: false,
                autoScroll: true,
                layout: 'hbox',
                items: [
                    {
                        xtype: 'datefield',
                        fieldLabel: '申請日期',
                        labelWidth: 90,
                        width: 170,
                        editable: false,
                        name: 'P5',
                        listeners: {
                            change: function () {
                                Ext.getCmp('t1print').setDisabled(true);
                                Ext.getCmp('t1export').setDisabled(true);
                            }
                        }
                    },
                    {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelWidth: 20,
                        width: 100,
                        editable: false,
                        name: 'P6',
                        labelSeparator: '',
                        listeners: {
                            change: function () {
                                Ext.getCmp('t1print').setDisabled(true);
                                Ext.getCmp('t1export').setDisabled(true);
                            }
                        }
                    }, {
                        xtype: 'datefield',
                        fieldLabel: '核撥日期',
                        labelWidth: 90,
                        width: 170,
                        //editable: false,
                        name: 'P7',
                        listeners: {
                            change: function () {
                                Ext.getCmp('t1print').setDisabled(true);
                                Ext.getCmp('t1export').setDisabled(true);
                            }
                        }
                    },
                    {
                        xtype: 'datefield',
                        fieldLabel: '至',
                        labelWidth: 20,
                        width: 100,
                        //editable: false,
                        name: 'P8',
                        labelSeparator: '',
                        listeners: {
                            change: function () {
                                Ext.getCmp('t1print').setDisabled(true);
                                Ext.getCmp('t1export').setDisabled(true);
                            }
                        }
                    }
                    
                ]
            }, {
                xtype: 'panel',
                id: 'PanelP2',
                border: false,
                autoScroll: true,
                layout: 'hbox',
                items: [{
                    xtype: 'combo',
                    fieldLabel: '物料分類',
                    labelWidth: 65,
                    width: 170,
                    name: 'P0',
                    id: 'P0',
                    store: CLSNAMEStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    listeners: {
                        change: function () {
                            T1Query.getForm().findField("P1").setValue("");
                            Ext.getCmp('t1print').setDisabled(true);
                            Ext.getCmp('t1export').setDisabled(true);
                        }
                    }
                },
                {
                    xtype: 'combo',
                    fieldLabel: '申請單狀態',
                    labelWidth: 80,
                    width: 185,
                    name: 'P4',
                    id: 'P4',
                    store: FLOWIDStore,
                    queryMode: 'local',
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    autoSelect: true,
                    anyMatch: true,
                    editable: false,
                    listeners: {
                        change: function () {
                            Ext.getCmp('t1print').setDisabled(true);
                            Ext.getCmp('t1export').setDisabled(true);
                        }
                    }
                },
                    mmCodeCombo,
                {
                    xtype: 'displayfield',
                    fieldLabel: '庫房別',
                    name: 'WH_NO',
                    fieldStyle: 'color:blue; font-weight:bold;',
                    enforceMaxLength: true,
                    labelWidth: 55,
                    width: 165,
                    padding: '0 4 0 4',
                    //store: Wh_noQueryStore,                            
                    //displayField: 'TEXT',
                    //valueField: 'VALUE',
                    queryMode: 'local',
                    anyMatch: true,
                    //fieldCls: 'required',
                    //readOnly: true,
                    listeners: {
                        change: function () {
                            Ext.getCmp('t1print').setDisabled(true);
                            Ext.getCmp('t1export').setDisabled(true);
                        }
                    },
                    editable: false
                },
                {
                    xtype: 'button',
                    text: '查詢',
                    //margin: '0 4 0 30',
                    handler: function () {
                        msglabel('訊息區:');
                        T1Load();
                        Ext.getCmp('t1print').setDisabled(false);
                        Ext.getCmp('t1export').setDisabled(false);
                    }
                },
                {
                    xtype: 'button',
                    text: '清除',
                    //margin: '0 4 0 4',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0                         
                        Ext.getCmp('t1print').setDisabled(true);
                        Ext.getCmp('t1export').setDisabled(true);
                        f.findField('WH_NO').setValue(getWhName);
                    }
                }]
            }
        ]
    });

    var T1Store = Ext.create('WEBAPP.store.AA.AA0063VM', { // 定義於/Scripts/app/store/AA/AA0063VM.js
        listeners: {
            beforeload: function (store, options) {
                if (T1Query.getForm().findField('P0').getValue() !== '') {
                    // 載入前將查詢條件P0~P5的值代入參數
                    var np = {
                        p0: T1Query.getForm().findField('P0').getValue(),   //物料分類
                        p1: T1Query.getForm().findField('P1').getValue(),   //院內碼
                        //p2: T1Query.getForm().findField('P2').getValue(),   //庫房代碼
                        p3: false,                                              //物料分類是否全選
                        p4: T1Query.getForm().findField('P4').getValue(),   //申請單狀態
                        p5: T1Query.getForm().findField('P5').rawValue,   //從日期
                        p6: T1Query.getForm().findField('P6').rawValue,   //至日期
                        p7: T1Query.getForm().findField('P7').rawValue,   //從日期
                        p8: T1Query.getForm().findField('P8').rawValue   //至日期
                    };
                }
                else {
                    parCLS = "";
                    for (var i = 0; i < tempCLS.length; i++) {
                        parCLS += tempCLS[i].VALUE + ',';
                    };

                    // 載入前將查詢條件P0~P5的值代入參數
                    var np = {
                        p0: parCLS,                                             //物料分類
                        p1: T1Query.getForm().findField('P1').getValue(),   //院內碼
                        //p2: T1Query.getForm().findField('P2').getValue(),   //庫房代碼
                        p3: true,                                               //物料分類是否全選
                        p4: T1Query.getForm().findField('P4').getValue(),   //申請單狀態
                        p5: T1Query.getForm().findField('P5').rawValue,   //從日期
                        p6: T1Query.getForm().findField('P6').rawValue,   //至日期
                        p7: T1Query.getForm().findField('P7').rawValue,   //從日期
                        p8: T1Query.getForm().findField('P8').rawValue   //至日期
                    };
                }

                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                id: 't1print', text: '列印', disabled: true,
                handler: function () {
                    if (T1Store.getCount() > 0)
                        showReport();
                    else
                        Ext.Msg.alert('訊息', '請先建立報表資料。');
                }
            },
            {
                id: 't1export', text: '匯出', disabled: true,
                handler: function () {
                    var today = getTodayDate();
                    var p = new Array();
                    var p3 = '';
                    if (T1Query.getForm().findField('P0').getValue() !== '') {
                        p3 = false;
                        exportCLS = T1Query.getForm().findField('P0').getValue();
                    }
                    else {
                        p3 = true;
                        exportCLS = parCLS;
                    };
                    p.push({ name: 'FN', value: today + '_核撥明細表.xls' });
                    p.push({ name: 'P0', value: exportCLS });
                    p.push({ name: 'P1', value: T1Query.getForm().findField('P1').getValue() });
                    //p.push({ name: 'P2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'P3', value: p3 });
                    p.push({ name: 'P4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'P5', value: T1Query.getForm().findField('P5').rawValue });
                    p.push({ name: 'P6', value: T1Query.getForm().findField('P6').rawValue });
                    p.push({ name: 'P7', value: T1Query.getForm().findField('P7').rawValue });
                    p.push({ name: 'P8', value: T1Query.getForm().findField('P8').rawValue });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }
        ]
    });


    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
                //autoScroll: true,
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer',
            }, {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 140
            },
            //{
            //    text: "申請單位",
            //    dataIndex: 'INID_NAME',
            //    width: 140
            //},
            {
                text: "入庫庫房",
                dataIndex: 'TOWH',
                width: 140
            },
            {
                text: "申請單狀態",
                dataIndex: 'FLOWID',
                //renderer: function (value) {
                //    switch (value) {
                //        case '1': return '1-申請中'; break;
                //        case '2': return '2-核撥中'; break;
                //        case '3': return '3-揀料中'; break;
                //        case '4': return '4-點收中'; break;
                //        case '5': return '5-已點收'; break;
                //        case '6': return '6-已核撥確認'; break;
                //        default: return value; break;
                //    }
                //},
                width: 80
            }, {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 80
            },
            {
                text: "庫備否",
                dataIndex: 'M_STOREID',
                width: 80
            },
            //{
            //text: "庫房",
            //dataIndex: 'FRWH',
            //width: 160
            //},
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 70
            }, {
                text: "核撥日期",
                dataIndex: 'DIS_TIME',
                width: 70
            }, {
                text: "申請單備註",
                dataIndex: 'APPLY_NOTE',
                width: 80
            }, {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 70
            }, {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 250
            }, {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 250
            }, {
                text: "申請數量",
                dataIndex: 'APPQTY',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "核撥數量",
                dataIndex: 'dis_qty',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                text: "單位",
                dataIndex: 'BASE_UNIT',
                width: 40
            }, {
                text: "最小單價",
                dataIndex: 'M_CONTPRICE',
                style: 'text-align:left',
                align: 'right',
                width: 70
            }, {
                header: "",
                flex: 1
            }],
    });

    var viewport = Ext.create('Ext.Viewport', {
        renderTo: body,
        layout: {
            type: 'border',
            padding: 0
        },
        defaults: {
            split: true  //可以調整大小
        },
        items: [
            {
                itemId: 't1Grid',
                region: 'center',
                layout: 'fit',
                collapsible: false,
                title: '',
                border: false,
                //height: '40%',
                split: true,
                items: [T1Grid]
            }
        ]
    });

    function showReport() {
        if (!win) {
            var p3 = '';
            if (T1Query.getForm().findField('P0').getValue() !== '') {
                p3 = false;
                printCLS = T1Query.getForm().findField('P0').getValue();
            }
            else {
                p3 = true;
                printCLS = parCLS;
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                    + '?MAT_CLASS=' + printCLS
                    + '&MMCODE=' + T1Query.getForm().findField('P1').getValue()
                    + '&FRWH=' + WHNO_MM1
                    + '&clsALL=' + p3
                    + '&FLOWID=' + T1Query.getForm().findField('P4').getValue()
                    + '&PR_TIME_B=' + T1Query.getForm().findField('P5').rawValue
                    + '&PR_TIME_E=' + T1Query.getForm().findField('P6').rawValue
                    + '&DIS_TIME_B=' + T1Query.getForm().findField('P7').rawValue
                    + '&DIS_TIME_E=' + T1Query.getForm().findField('P8').rawValue
                    + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                    }
                }]
            });
            var win = GetPopWin(viewport, winform, '', viewport.width - 300, viewport.height - 20);
        }
        win.show();
    }

    function T1Load() {
        T1Tool.moveFirst();
    }

    T1Query.getForm().findField('P0').focus();
    SetCLSNAME();
    SetFLOWID();

    var d = new Date();
    m = d.getMonth(); //current month
    y = d.getFullYear(); //current year

    T1Query.getForm().findField('P5').setValue(new Date().addMonth(-1));
    T1Query.getForm().findField('P6').setValue(new Date());

    T1Grid.down('#t1print').setDisabled(true);
    T1Grid.down('#t1export').setDisabled(true);

});
