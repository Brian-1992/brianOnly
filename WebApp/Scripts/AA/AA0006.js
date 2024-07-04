Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1LastRec = null, T2LastRec = null;
    var DisConfirmSet = '/api/AA0006/UpdateDetail';

    var ackDetailWindowTitle = '';
    var viewModel = Ext.create('WEBAPP.store.AA.AA0006VM');

    //物料分類Store
    var st_MatClass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0006/GetMatClassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        listeners: {
            load: function (store, options) {
                var MatClassCount = store.getCount();
                var combo_P0 = T1Query.getForm().findField('P0');
                if (MatClassCount > 0) {
                    combo_P0.setValue(store.getAt(0).get('VALUE'));
                }
            }
        },
        autoLoad: true
    });

    //廠商編號Store
    var st_AGEN_NO = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0006/GetAGEN_NO',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var T1Store = viewModel.getStore('AA0006M');
    function T1Load() {
        T2Store.removeAll();
        T2LastRec = null;
        Ext.getCmp('btnCheck').setDisabled(true);

        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getRawValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getRawValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("p6", T1Query.getForm().findField('P6').getValue());
        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('AA0006D');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["PO_NO"] !== '') {
            T2Store.getProxy().setExtraParam("PO_NO", T1LastRec.data["PO_NO"]);
            T2Store.getProxy().setExtraParam("MMCODE", T1LastRec.data["MMCODE"]);
            T2Store.getProxy().setExtraParam("SEQ", T1LastRec.data["SEQ"]);
            T2Tool.moveFirst();
        }
    }
    var T1QuryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P5',
        name: 'P5',
        fieldLabel: '院內碼(品項名稱)',
        labelAlign: 'right',
        labelWidth: 100,
        width: 300,
        limit: 200, //限制一次最多顯示10筆
        //fieldCls: 'required',
        //allowBlank: false,
        queryUrl: '/api/AA0006/GetMMCODECombo', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                mat_class: T1Query.getForm().findField('P0').getValue()  //P0:預設是動態MMCODE
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        },
    });
    // 是否分配
    var store_dist = Ext.create('Ext.data.Store', {
        fields: ['TEXT', 'VALUE'],
        data: [
            { TEXT: '全部', VALUE: '' },
            { TEXT: '待分配', VALUE: 'L' },
            { TEXT: '已分配', VALUE: 'C' }
        ]
    });

    // 查詢欄位
    var mLabelWidth = 70;
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
        items: [{
            xtype: 'container',
            layout: 'vbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    bodyStyle: 'padding: 3px 5px;',
                    items: [
                        {
                            xtype: 'combo',
                            fieldLabel: '物料分類',
                            store: st_MatClass,
                            name: 'P0',
                            id: 'P0',
                            labelWidth: 80,
                            width: 240,
                            queryMode: 'local',
                            fieldCls: 'required',
                            allowBlank: false,
                            displayField: 'COMBITEM',
                            valueField: 'VALUE'
                        }, {
                            xtype: 'datefield',
                            fieldLabel: '接收日期',
                            name: 'P1',
                            id: 'P1',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 80,
                            width: 160,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls: 'required',
                            value: Ext.util.Format.date(new Date(), "Y-m-") + "01",
                            regexText: '請選擇日期'
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            labelSeparator: '',
                            name: 'P2',
                            id: 'P2',
                            labelWidth: mLabelWidth,
                            width: 88,
                            labelWidth: 8,
                            padding: '0 2 0 2',
                            allowBlank: false,
                            fieldCls: 'required',
                            value: new Date(),
                            regexText: '請選擇日期'
                        },
                        {
                            xtype: 'combo',
                            fieldLabel: '是否分配',
                            store: store_dist,
                            name: 'P6',
                            id: 'P6',
                            labelWidth: 70,
                            width: 150,
                            queryMode: 'local',
                            fieldCls: 'required',
                            allowBlank: false,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            value: 'L'
                        }, T1QuryMMCode, {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                if (T1Query.getForm().isValid()) {
                                    T1Load();
                                    T2Grid.getStore().removeAll();
                                } else {
                                    Ext.Msg.alert('提醒', '查詢條件要輸入完整');
                                }
                            }
                        }
                    ]
                },
            ]
        }]
    });

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        plain: true,
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [T1RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                layout: 'fit',
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
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 180
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 80
            },
            {
                text: "進貨接收量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'ACC_QTY',
                width: 90
            },
            {
                text: "計量單位",
                dataIndex: 'ACC_BASEUNIT',
                width: 75
            },
            //{
            //    text: "借貨量",
            //    style: 'text-align:left',
            //    align: 'right',
            //    dataIndex: 'BW_SQTY',
            //    width: 70
            //},
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 120
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 120
            },
            {
                text: "狀態",
                dataIndex: 'STATUS',
                width: 80
            },
            {
                text: "接收人員",
                dataIndex: 'ACC_USER',
                width: 90
            },
            {
                text: "接收日期",
                dataIndex: 'ACC_TIME',
                width: 110
            },
            {
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 110
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');
                T1LastRec = record;
                T2Load();
                //if (T1LastRec.data["STATUS"] != 'P:完成過帳') {
                //    Ext.getCmp('btnCheck').setDisabled(true);
                //}
                //else {
                //    Ext.getCmp('btnCheck').setDisabled(false);
                //}
            }
        }
    });

    var callableWin = null;
    //數量檢核確認
    popCheckConfirm = function () {
        var store = T2Grid.getStore();
        var records = store.getRange(); // 全部筆數
        var data = [];

        let Update_BW_SQTY = '';
        let Update_DIST_QTY = '';
        let Update_DIST_MEMO = '';
        let Update_PO_NO = '';
        let Update_MMCODE = '';
        let Update_SEQ = '';
        let Update_PR_DEPT = '';

        Ext.Array.each(records, function (model) {
            data.push(model.data);
        });

        var allValid = false;
        var invalidList = '';
        if (records.length > 0) {
            for (var i = 0; i < records.length; i++) {
                var tmpDetail_DIST_QTY = records[i].data.DIST_QTY;      //Detail分配量
                var tmpDetail_BW_SQTY = 0;        //Detail借貨量
                var tmpDetail_PR_QTY = records[i].data.PR_QTY;          //Detail申請數量
                var tmpMaster_BW_SQTY = 0;      //Master借貨量
                var tmpDetail_BW_SQTY_SUM = 0;     //Detail借貨量總和
                var tmpMaster_ACC_QTY = T1LastRec.data["ACC_QTY"];      //Master進貨接收量
                var tmpDetail_DIST_QTY_SUM = Ext.util.Format.number(T2Store.sum('DIST_QTY'), '0');   //Detail分配量總和

                // Detail分配量 + Detail借貨量 <= Detail申請數量
                var IsCondition_1_OK = tmpDetail_DIST_QTY + tmpDetail_BW_SQTY <= tmpDetail_PR_QTY ? true : false;
                // Detail借貨量 = Detail借貨量總和
                var IsCondition_2_OK = tmpMaster_BW_SQTY == tmpDetail_BW_SQTY_SUM ? true : false;
                // Master進貨接收量 = Detail分配量總和
                var IsCondition_3_OK = tmpMaster_ACC_QTY == tmpDetail_DIST_QTY_SUM ? true : false; 

                if (IsCondition_1_OK && IsCondition_2_OK && IsCondition_3_OK) {

                    Update_BW_SQTY += records[i].data.BW_SQTY + ',';
                    Update_DIST_QTY += records[i].data.DIST_QTY + ',';
                    Update_DIST_MEMO += records[i].data.DIST_MEMO + ',';
                    Update_PO_NO += records[i].data.PO_NO + ',';
                    Update_MMCODE += records[i].data.MMCODE + ',';
                    Update_SEQ += records[i].data.SEQ + ',';
                    Update_PR_DEPT += records[i].data.PR_DEPT + ',';
                }
                else {
                    invalidList += "</span><span style=\'color:red\'>";
                    invalidList += "下方窗格第" + (i + 1) + "列 ";
                    if (!IsCondition_1_OK) {
                        invalidList += "條件(a)不符合 ";
                    }
                    if (!IsCondition_2_OK) {
                        invalidList += "條件(b)不符合 ";
                    }
                    // 2023-03-16: 配合庫房縮減取消判斷分配量總和與進貨接收量需相等條件
                    // 2023-04-07: 測試進貨調整功能先開放
                    if (!IsCondition_3_OK) {
                        invalidList += "條件(c)不符合 ";
                    }
                    invalidList += "請再進行數量檢核</span><br>";
                }
            }
            if (invalidList == '') {
                allValid = true;
            }
        } else {
            return false;
        }

        var msgContent = '<p style ="line-height:150%"><font size="3vmin">數量檢核說明：<br>'
            + '(a)下方窗格中每一列，分配量<=申請數量。<br>'
            + '(b)上方窗格中選取列的進貨接收量 = 下方窗格中每一列分配量總和。<br>'
            + '<br>'
            + invalidList
            + '<br></font></p>';

        if (!callableWin) {
            var popMainform = Ext.create('Ext.panel.Panel', {
                height: '100%',
                closable: false,
                plain: true,
                loadMask: true,
                layout: 'fit',
                items: [{
                    xtype: 'form',
                    id: 'Checkform',
                    height: '100%',
                    layout: 'fit',
                    padding: '4 4 4 4',
                    closable: false,
                    border: false,
                    fieldDefaults: {
                        labelAlign: 'right',
                        labelWidth: false,
                        labelStyle: 'width: 35%',
                        width: '95%'
                    },
                    items: [
                        {
                            xtype: 'container',
                            layout: 'vbox',
                            padding: '2vmin',
                            scrollable: true,
                            items: [
                                {
                                    xtype: 'label',
                                    name: 'CheckMsg',
                                    html: msgContent
                                }
                            ]
                        }
                    ]
                }],
                buttons: [{
                    id: 'Btn_CheckConfirm',
                    itemId: 'Btn_CheckConfirm',
                    disabled: true,
                    text: '確認',
                    height: '6vmin',
                    handler: function () {
                        T2UpdateSubmit(Update_BW_SQTY, Update_DIST_QTY, Update_DIST_MEMO,
                            Update_PO_NO, Update_MMCODE, Update_SEQ, Update_PR_DEPT);
                    }
                }, {
                    id: 'winclosed',
                    disabled: false,
                    text: '取消',
                    height: '6vmin',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });

            callableWin = GetPopWin(viewport, popMainform, '分配確認：數量檢核確認', viewport.width * 0.6, viewport.height * 0.6);

            popMainform.down('#Btn_CheckConfirm').setDisabled(false);
        }
        callableWin.show();

        if (allValid)
            Ext.ComponentQuery.query('button[itemId=Btn_CheckConfirm]')[0].setDisabled(false);
        else
            Ext.ComponentQuery.query('button[itemId=Btn_CheckConfirm]')[0].setDisabled(true);
    }

    function T2UpdateSubmit(Update_BW_SQTY, Update_DIST_QTY, Update_DIST_MEMO,
        Update_PO_NO, Update_MMCODE, Update_SEQ, Update_PR_DEPT) {
        Ext.Ajax.request({
            url: DisConfirmSet,
            params: {
                BW_SQTY: Update_BW_SQTY,
                DIST_QTY: Update_DIST_QTY,
                DIST_MEMO: Update_DIST_MEMO,
                PO_NO: Update_PO_NO,
                MMCODE: Update_MMCODE,
                SEQ: Update_SEQ,
                PR_DEPT: Update_PR_DEPT
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Store.load();
                    msglabel('分配確認完成');
                    callableWin.destroy();
                    callableWin = null;
                    Ext.getCmp('btnCheck').setDisabled(true);
                    //alert(data.msg);
                }
                else {
                    Ext.Msg.alert('訊息', '<span style=\'color:red\'>' + data.msg + '</span>');
                }
            },
            failure: function (response, options) {
                var data = Ext.decode(response.responseText);
                Ext.Msg.alert('訊息', '<span style=\'color:red\'>' + data.msg + '</span>');
            }
        });
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        emptyMsg: '沒有任何資料     <span style=\'color:blue\'>預設[分配量] = [申請數量]</span><span style=\'color:red\'>   紅色資料表示[分配量]<>[申請數量]</span>',
        displayMsg: "顯示{0} - {1}筆,共{2}筆    <span style=\'color:blue\'>預設[分配量] = [申請數量]</span><span style=\'color:red\'>   紅色資料表示[分配量]<>[申請數量]</span>",
        buttons: [
            {
                text: '分配確認',
                id: 'btnCheck',
                name: 'btnCheck',
                handler: function () {
                    
                    var tempData = T2Grid.getStore().data.items;
                    var has0 = false;
                    for (var i=0; i < tempData.length; i++) {
                        if (Number(tempData[i].data.DIST_QTY) == 0) {
                            has0 = true;
                        }
                    }
                    if (has0) {
                        Ext.MessageBox.confirm('', '包含分配量為0資料，是否確定？', function (btn, text) {
                            if (btn === 'yes') {
                                popCheckConfirm();
                                msglabel('');
                            }
                        }
                        );
                    } else {
                        popCheckConfirm();
                        msglabel('');
                    }

                   
                }
            },
            {
                xtype: 'displayfield',
                fieldLabel: '',
                value: '<span style="color:blue">輸入分配量後請點暫存再確認分配</span>',
                padding: '0 0 0 20'
            }
        ]
    });

    var T2RowEditing = Ext.create('Ext.grid.plugin.RowEditing', {
        //clicksToMoveEditor: 1,
        clicksToEdit: 1,
        autoCancel: false,
        saveBtnText: '暫存',
        cancelBtnText: '取消',
        errorsText: '錯誤訊息',
        dirtyText: '請按暫存以暫存資料或取消變更',
        listeners: {
            beforeedit: function (editor, context, eOpts) {
                if (T1LastRec.data["STATUS"] != 'P:完成過帳') {
                    return false;  // 狀態不等於P:完成過帳，取消row editing模式   
                }
                if (context.record.data['DIST_STATUS'].indexOf('T') > -1) {
                    Ext.getCmp('btnCheck').setDisabled(true);
                    return false;  // 狀態不等於T:待點收, 完成過帳，取消row editing模式   
                }
                else {
                    Ext.getCmp('btnCheck').setDisabled(false);
                }
            }
        }
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        plugins: [T2RowEditing],
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80,
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "申請部門",
                dataIndex: 'INID_NAME',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "申請數量",
                style: 'text-align:left',
                align: 'right',
                dataIndex: 'PR_QTY',
                width: 80,
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "已分配量",
                dataIndex: 'SUMDIST_QTY',
                width: 80
            },
            {
                text: "累計點收量",
                dataIndex: 'SUM_ACKQTY',
                width: 80,
                renderer: function (val, meta, record) {
                    if (val != null) {
                        return '<a href=javascript:void(0)>' + val + '</a>';
                    }
                }
            },
            {
                text: "計量單位",
                dataIndex: 'DIST_BASEUNIT',
                width: 80,
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "狀態",
                dataIndex: 'DIST_STATUS',
                width: 90,
                style: 'text-align:left',
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            //{
            //    text: "借貨量",
            //    style: 'text-align:left; color:red',
            //    align: 'right',
            //    dataIndex: 'BW_SQTY',
            //    width: 70,
            //    editor: {
            //        xtype: 'textfield',
            //        regexText: '只能輸入數字',
            //        regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            //        listeners: {

            //        }
            //    },
            //    renderer: function (val, meta, record) {
            //        if (record.data["BW_SQTY"] + record.data["DIST_QTY"] != record.data["PR_QTY"]) {
            //            return '<font color="red">' + val + '</font>';
            //        }
            //        else {
            //            return '<font color="black">' + val + '</font>';
            //        }
            //    }
            //},
        {
                text: "分配量",
                style: 'text-align:left; color:red',
                align: 'right',
                dataIndex: 'DIST_QTY',
                width: 70,
                editor: {
                    xtype: 'numberfield',
                    decimalPrecision: 0,
                    hideTrigger:true,
                    //regexText: '只能輸入數字',
                    //regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
                    //listeners: {

                    //}
                },
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "備註",
                style: 'color:red',
                dataIndex: 'DIST_MEMO',
                width: 120,
                editor: {
                    xtype: 'textfield',
                    listeners: {

                    }
                },
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "批號",
                dataIndex: 'LOT_NO',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 100,
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 150,
                renderer: function (val, meta, record) {
                    if (record.data["DIST_QTY"] != record.data["PR_QTY"]) {
                        return '<font color="red">' + val + '</font>';
                    }
                    else {
                        return '<font color="black">' + val + '</font>';
                    }
                }
            }, {
                text: "申請責任中心",
                dataIndex: 'PR_DEPT',
                width: 100
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                msglabel('訊息區:');
                T2LastRec = record;
            },
            selectionchange: function (grid, col, e) {
                msglabel('訊息區:');
            },
            cellclick: function (grid, td, cell, rec, tr, row, ev) {
                grid.editingPlugin.on({
                    beforeedit: function (plugin, e) {
                        e.cancel = (cell === 0);
                    }
                });
                var columnIndex = grid.getHeaderCt().getHeaderAtIndex(cell).config.dataIndex;
                if (columnIndex != 'SUM_ACKQTY') {
                    return;
                }
                
                ackStoreLoad(rec.data.DOCNO, rec.data.MMCODE);
                ackDetailWindow.setTitle('申請部門：' + rec.data.INID_NAME + ' 申請單號：' + rec.data.DOCNO + ' 申請責任中心：' + rec.data.PR_DEPT);
                ackDetailWindow.show();
            }
        }
    });

    //#region 2021-11-24 顯示點收明細
    var ackStore = Ext.create('Ext.data.Store', {
        fields: ['TR_DATE', 'MMCODE', 'TR_INV_QTY']
    });
    function ackStoreLoad(docno, mmcode) {
        Ext.Ajax.request({
            url: '/api/AA0006/GetAckDetails',
            params: {
                docno: docno,
                mmcode: mmcode
            },
            method: reqVal_p,
            success: function (response) {
                ackStore.removeAll();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    for (var i = 0; i < data.etts.length; i++) {
                        ackStore.add(data.etts[i]);
                    }
                }
            },
            failure: function (response, options) {
                var data = Ext.decode(response.responseText);
                Ext.Msg.alert('訊息', '<span style=\'color:red\'>' + data.msg + '</span>');
            }
        });
    }
  
    var ackGrid = Ext.create('Ext.grid.Panel', {
        store: ackStore,
        id: 'ackGrid',
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [
            {
                text: "交易時間",
                dataIndex: 'TR_DATE',
                width: 120
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 80
            },
            {
                text: "點收量",
                dataIndex: 'TR_INV_QTY',
                style: 'text-align:left',
                width: 80
            }
        ],
    });
    var ackDetailWindow = Ext.create('Ext.window.Window', {
        id: 'ackDetailWindow',
        renderTo: Ext.getBody(),
        items: [ackGrid],
        modal: true,
        width: "550px",
        height: 200,
        resizable: true,
        draggable: true,
        closable: false,
        //x: ($(window).width() / 2) - 300,
        y: 0,
        buttons: [
            {
                text: '關閉',
                handler: function () {
                    ackDetailWindow.hide();
                }
            }],
        listeners: {
            show: function (self, eOpts) {
                ackDetailWindow.center();
                ackDetailWindow.setWidth(550);
            }
        }
    });
    //#endregion

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
                itemId: 't1top',
                region: 'center',
                layout: 'border',
                collapsible: false,
                title: '',
                border: false,
                items: [
                    {
                        itemId: 't1Grid',
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        border: false,
                        height: '50%',
                        split: true,
                        items: [T1Grid]
                    },
                    {
                        itemId: 't2Grid',
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }
        ]
    });

    Ext.getCmp('btnCheck').setDisabled(true);
});