Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Rec;
    var T1Name = '繳回申請'; // 退料作業
    var reportUrl = '/Report/A/AB0112.aspx';
    var GetLOT_NO = '../../../api/AB0112/GetLOT_NO';
    //var GetEXP_DATE = '../../../api/AB0112/GetEXP_DATE';

    var arrP2 = [];
    //var arrP3 = ["03", "04", "05", "06", "07"];
    var userTaskId;
    var userId = session["UserId"];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];


    //繳回單號Store
    var DocnoStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    //單據狀態Store
    var FlowIdStore = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0112/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, records, successful, eOpts) {
                for (var i = 0; i < records.length; i++) {
                    // flowid尾碼為1或2的當作預設值
                    if (records[i].data.VALUE.slice(-1) == '1' || records[i].data.VALUE.slice(-1) == '2')
                        arrP2.push(records[i].data.VALUE);
                }
                T1Query.getForm().findField('P2').setValue(arrP2);
            }
        }
    });
    //T1Form的物料分類Store
    var MatClassStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    //T1Query的物料分類Store
    var MatClassStoreQ = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    //取得物料分類Combo
    function MatClassLoad() {
        Ext.Ajax.request({
            url: '/api/AB0112/GetMatClassCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var records = data.etts;
                    if (records.length > 0) {
                        MatClassStore.removeAll();
                        for (var i = 0; i < records.length; i++) {
                            MatClassStore.add({
                                VALUE: records[i].VALUE,
                                TEXT: records[i].TEXT,
                                COMBITEM: records[i].COMBITEM
                            });
                        }
                    }
                }
            }
        });

        Ext.Ajax.request({
            url: '/api/AB0112/GetMatClassQCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var records = data.etts;
                    if (records.length > 0) {
                        MatClassStoreQ.removeAll();
                        for (var i = 0; i < records.length; i++) {
                            MatClassStoreQ.add({
                                VALUE: records[i].VALUE,
                                TEXT: records[i].TEXT,
                                COMBITEM: records[i].COMBITEM
                            });
                        }
                    }
                }
            }
        });
    }
    //出庫庫房combo
    var st_frwhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0112/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    //入庫庫房combo
    var st_towhcombo = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM']
    });
    //院內碼
    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            mmCodeCombo.doQuery();
            var func = function () {
                var record = mmCodeCombo.store.getAt(0);
                mmCodeCombo.select(record);
                mmCodeCombo.fireEvent('select', this, record);
                mmCodeCombo.store.un('load', func);
            };
            mmCodeCombo.store.on('load', func);
        }
    }
    var mmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'MMCODE',
        fieldLabel: '院內碼',
        fieldCls: 'required',
        allowBlank: false,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AB0112/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT', 'INV_QTY'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T2Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                var docno2 = f.findField("DOCNO2").getValue();
                return {
                    DOCNO: docno2
                };
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                var f = T2Form.getForm();
                f.findField("LOT_NO").setValue("");
                f.findField("LOT_NO").clearInvalid();
                f.findField("EXP_DATE").setValue("");

                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                    f.findField("INV_QTY").setValue(r.get('INV_QTY'));
                    SetLOT_NO(r.get('MMCODE'));
                }
                else {
                    f.findField("MMCODE").setValue("");
                    f.findField("MMNAME_C").setValue("");
                    f.findField("MMNAME_E").setValue("");
                    f.findField("BASE_UNIT").setValue("");
                    f.findField("INV_QTY").setValue("");
                }
                f.findField("APVQTY").setValue("0");
            }
        }
    });
    //批號
    function setLotno(args) {
        var f = T2Form.getForm();
        if (args.LOTNO !== '') {
            f.findField("LOT_NO").setValue(args.LOT_NO);
            //f.findField("LOT_NO_N").setValue(args.LOT_NO);
            f.findField("EXP_DATE").setValue(args.EXP_DATE);
            //f.findField("EXP_DATE_T").setValue(args.EXP_DATE);
            f.findField("INV_QTY").setValue(args.INV_QTY);
        }
    }
    //批號的store
    var LOT_NO_Store = Ext.create('Ext.data.Store', {
        fields: ['LOT_NO', 'EXP_DATE', 'INV_QTY']
    });
    //取得批號
    function SetLOT_NO(MMCODE) { //建立批號的下拉式選單
        var qty_temp = 0;
        Ext.Ajax.request({
            url: GetLOT_NO,
            method: reqVal_p,
            params: {
                FRWH: T1Form.getForm().findField("FRWH").getValue(),
                MMCODE: MMCODE
            },
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var lot_nos = data.etts;
                    LOT_NO_Store.removeAll();
                    if (lot_nos.length > 0) {
                        LOT_NO_Store.add({ LOT_NO: '', EXP_DATE: '', INV_QTY: '' });
                        for (var i = 0; i < lot_nos.length; i++) {
                            LOT_NO_Store.add({ LOT_NO: lot_nos[i].LOT_NO, EXP_DATE: lot_nos[i].EXP_DATE, INV_QTY: lot_nos[i].INV_QTY });
                            T2Form.getForm().findField("LOT_NO").setValue(lot_nos[0].LOT_NO);
                            T2Form.getForm().findField("EXP_DATE").setValue(lot_nos[0].EXP_DATE);
                            //T2Form.getForm().findField("EXP_DATE_T").setValue(lot_nos[0].EXP_DATE);
                            T2Form.getForm().findField("INV_QTY").setValue(lot_nos[0].INV_QTY);

                            T2Form.getForm().findField("APVQTY").setValue(T2Form.getForm().findField("INV_QTY").getValue());
                            if (T2Form.getForm().findField("APVQTY").getValue() == null) {
                                T2Form.getForm().findField("APVQTY").setValue(qty_temp);
                            }
                        }
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }


    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 230;
    //定義有多少欄位參數
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },      // 繳回單號
            { name: 'MAT_CLASS', type: 'string' },  // 物料分類
            { name: 'APPTIME', type: 'string' },    // 繳回申請時間
            { name: 'TOWH', type: 'string' },       // 入庫庫房
            { name: 'FRWH', type: 'string' },       // 出庫庫房
            { name: 'FLOWID', type: 'string' }     // 單據狀態
        ]
    });
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
        items: [
            {
                xtype: 'displayfield',
                fieldLabel: '繳回單號',
                store: DocnoStore,
                displayField: 'DOCNO',
                valueField: 'DOCNO',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4',
                anyMatch: true,
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                hidden: true
            },
            {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P3',
                id: 'P3',
                store: MatClassStoreQ,
                labelWidth: 90,
                editable: false,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            },
            {
                xtype: 'datefield',
                fieldLabel: '繳回日期',
                name: 'D0',
                id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4',
                value: new Date().addMonth(-1)
            },
            {
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'D1',
                id: 'D1',
                labelSeparator: '',
                regex: /\d{7,7}/,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4',
                value: new Date()
            },
            {
                xtype: 'combo',
                fieldLabel: '單據狀態',
                name: 'P2',
                id: 'P2',
                store: FlowIdStore,
                labelWidth: 90,
                editable: false,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }
        ],
        buttons: [
            {
                itemId: 'query',
                text: '查詢',
                handler: function () {
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    Ext.getCmp('eastform').collapse();
                    msglabel('訊息區:');

                    Ext.getCmp('btnUpdate').setDisabled(true);
                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    Ext.getCmp('btnAdd2').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnSubmitC').setDisabled(true);
                }
            },
            {
                itemId: 'clean',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    //f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');

                    T1Query.getForm().findField('P2').setValue(arrP2);
                }
            }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 20, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }], // 預設排序欄位
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0112/All',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd',
                name: 'btnAdd',
                handler: function () {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AB0112/MasterCreate';
                    setFormT1('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AB0112/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel',
                name: 'btnDel',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';

                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        });

                        Ext.MessageBox.confirm('刪除', '是否確定刪除繳回單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0112/MasterDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();

                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnUpdate2').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnDel2').setDisabled(true);
                                            Ext.getCmp('btnAdd2').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnSubmitC').setDisabled(true);

                                            T1Cleanup();
                                            T1Load();
                                        }
                                        else {
                                            Ext.MessageBox.alert('錯誤', data.msg);
                                            msglabel('訊息區:' + data.msg);
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            },
            {
                itemId: 'btnExp',
                id: 'btnExp',
                text: '效期',
                disabled: true,
                hidden: true,
                handler: function () {
                    showExpWindow(T1LastRec.data["DOCNO"], 'O', viewport);
                }
            },
            {
                text: '送繳回',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';
                        let apvqty = '';
                        let inv_qty = '';

                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                            apvqty += item.get('APVQTY') + ',';
                            inv_qty += item.get('INV_QTY') + ',';
                        });

                        Ext.MessageBox.confirm('訊息', '確認是否繳回?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0112/UpdateStatus',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        APVQTY: apvqty,
                                        INV_QTY: inv_qty
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:送出成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnSubmitC').setDisabled(true);
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
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取繳回單號');
                        return;
                    }
                }
            }, {
                text: '取消繳回',
                id: 'btnSubmitC',
                name: 'btnSubmitC',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        let name = '';
                        let docno = '';


                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        });

                        Ext.MessageBox.confirm('訊息', '確認是否取消繳回?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0112/UpdateStatus1',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:取消繳回成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnSubmitC').setDisabled(true);
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
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取取消繳回單號');
                        return;
                    }
                }
            }, {
                itemId: 'print', text: '列印', disabled: true, handler: function () {
                    showReport(T1LastRec.data["DOCNO"]);
                }
            }
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Query]
            },
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T1Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            allowDeselect: true
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "繳回單號",
                dataIndex: 'DOCNO',
                width: 150
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 120
            },
            {
                text: "繳回申請時間",
                dataIndex: 'APPTIME',
                width: 150
            },
            {
                text: "出庫庫房",
                dataIndex: 'FRWH',
                width: 180
            },
            {
                text: "入庫庫房",
                dataIndex: 'TOWH',
                width: 180
            },
            {
                text: "單據狀態",
                dataIndex: 'FLOWID',
                width: 150
            },
            {
                text: "備註",
                dataIndex: 'APPLY_NOTE',
                width: 150
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T1Form.hidden === true) {
                        T1Form.setVisible(true);
                        T2Form.setVisible(false);
                    }

                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T1LastRec != null) {
                        Ext.getCmp('btnUpdate').setDisabled(true);
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnExp').setDisabled(true);
                        Ext.getCmp('btnSubmit').setDisabled(true);
                        Ext.getCmp('btnSubmitC').setDisabled(true);
                        Ext.getCmp('btnUpdate2').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(true);

                        var tmpArray = T1LastRec.data["FLOWID"].split(' ');

                        // 修改：單筆狀態為申請中才可修改
                        if (T1Rec.length == 1) {
                            if (tmpArray[0] === '1' || tmpArray[0] === '0401') {   // 1-衛材申請中, 0401-藥材申請中
                                Ext.getCmp('btnUpdate').setDisabled(false);
                                Ext.getCmp('btnAdd2').setDisabled(false);
                            }
                            else {
                                Ext.getCmp('btnUpdate').setDisabled(true);
                                Ext.getCmp('btnAdd2').setDisabled(true);
                            }

                            // 效期：明細內有批號效期管制品項才enable
                            Ext.Ajax.request({
                                url: '/api/AB0112/CheckExp',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1LastRec.data["DOCNO"]
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        //Ext.getCmp('btnExp').setDisabled(false);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }

                        // 刪除：單筆或所有勾選資料狀態為申請中(1、0401)才可enable
                        // 送繳回：單筆或所有勾選資料狀態為申請中(1、0401)才可enable
                        var btnDelCheck = 0;
                        var btnSubmitCheck = 0;
                        var btnSubmitCCheck = 0;
                        for (var i = 0; i < T1Rec.length; i++) {
                            var tmpRecArray = T1Rec[i].data["FLOWID"].split(' ');
                            if (tmpRecArray[0] !== '1' && tmpRecArray[0] !== '0401') {    // 1-衛材申請中, 0401-藥材申請中
                                btnDelCheck++;
                                btnSubmitCheck++;
                            } else if (tmpRecArray[0] !== '2' && tmpRecArray[0] !== '0402') {    // 2-衛材繳回中, 0402-藥材點收中
                                btnSubmitCCheck++;
                            }
                        }
                        if (btnDelCheck == 0) {
                            Ext.getCmp('btnDel').setDisabled(false);
                        }
                        if (btnSubmitCheck == 0) {
                            Ext.getCmp('btnSubmit').setDisabled(false);
                        }
                        if (btnSubmitCCheck == 0) {
                            Ext.getCmp('btnSubmitC').setDisabled(false);
                        }
                        if (T1Set === '')
                            setFormT1a();

                        T2Load();
                    }
                }
            },
            selectionchange: function (model, records) {
                T1Rec = records;
                T1LastRec = records[records.length - 1];
                setFormT1a();
                T2Load();

                if (T1LastRec == null)
                    Ext.getCmp('btnAdd2').setDisabled(true);
            },
            deselect: function (model, record, index) {
                Ext.getCmp('btnUpdate').setDisabled(true);
                Ext.getCmp('btnDel').setDisabled(true);
                Ext.getCmp('btnExp').setDisabled(true);
                Ext.getCmp('btnSubmit').setDisabled(true);
                Ext.getCmp('btnSubmitC').setDisabled(true);
            }
        }
    });
    function T1Load() {
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());

        T1Store.getProxy().setExtraParam("TASKID", userTaskId);
        T1Store.getProxy().setExtraParam("APPDEPT", userInid);
        T1Tool.moveFirst();
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 90
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回單號',
                name: 'DOCNO',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '單據狀態',
                name: 'FLOWID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true,
                hidden: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回人員',
                name: 'APPID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回單位',
                name: 'INID_NAME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'combo',
                fieldLabel: '出庫庫房',
                name: 'FRWH',
                store: st_frwhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                triggerAction: 'all',
                fieldCls: 'required',
                listeners: {
                    select: function (combo, record, eOpts) {
                        var f = T1Form.getForm();
                        if (record.data.VALUE == '') {
                            f.findField('TOWH').disable();
                            f.findField('TOWH').setValue('');
                            f.findField('MAT_CLASS').disable();
                            f.findField('MAT_CLASS').setValue('');
                            return;
                        }
                        f.findField('TOWH').setValue('');
                        f.findField('TOWH').enable();
                        f.findField('MAT_CLASS').setValue('');
                        f.findField('MAT_CLASS').enable();

                        Ext.Ajax.request({
                            url: '/api/AB0112/GetTowhCombo',
                            method: reqVal_p,
                            params: {
                                Frwh_no: record.get("VALUE")
                            },
                            success: function (response) {
                                f.findField('TOWH').setReadOnly(false);
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    var records = data.etts;
                                    if (records.length > 0) {
                                        st_towhcombo.removeAll();
                                        for (var i = 0; i < records.length; i++) {
                                            st_towhcombo.add({
                                                VALUE: records[i].VALUE,
                                                TEXT: records[i].TEXT,
                                                COMBITEM: records[i].COMBITEM
                                            });
                                        }
                                        f.findField('TOWH').setValue(records[0].VALUE);
                                    }
                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });

                        Ext.Ajax.request({
                            url: '/api/AB0112/GetMatClassComboByFrwh',
                            method: reqVal_p,
                            params: {
                                Frwh_no: record.get("VALUE")
                            },
                            success: function (response) {
                                f.findField('MAT_CLASS').setReadOnly(false);
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    var records = data.etts;
                                    if (records.length > 0) {
                                        MatClassStore.removeAll();
                                        for (var i = 0; i < records.length; i++) {
                                            MatClassStore.add({
                                                VALUE: records[i].VALUE,
                                                TEXT: records[i].TEXT,
                                                COMBITEM: records[i].COMBITEM
                                            });
                                        }
                                        f.findField('MAT_CLASS').setValue(records[0].VALUE);
                                    }
                                }
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                        });
                    }
                }
            },
            {
                xtype: 'combo',
                fieldLabel: '入庫庫房',
                name: 'TOWH',
                store: st_towhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                anyMatch: true,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                typeAhead: true,
                forceSelection: true,
                triggerAction: 'all',
                fieldCls: 'required'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請日期',
                name: 'APPTIME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'MAT_CLASS',
                store: MatClassStore,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                readOnly: true,
                allowBlank: false, // 欄位為必填
                //anyMatch: true,
                //typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                //triggerAction: 'all',
                fieldCls: 'required'
            },
            {
                xtype: 'textareafield',
                fieldLabel: '備註',
                name: 'APPLY_NOTE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
            }
        ]
    });
    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        T1Grid.down('#print').setDisabled(T1Rec.length == 0);
        if (T1LastRec != null) {
            viewport.down('#form').expand();
            Tabs.setActiveTab('Form');
            var currentTab = Tabs.getActiveTab();
            currentTab.setTitle('瀏覽');
            var f = T1Form.getForm();
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            var tmpArray = T1LastRec.data["FRWH"].split(" ");
            f.findField("FRWH").setValue(tmpArray[0]);
            tmpArray = T1LastRec.data["TOWH"].split(" ");
            st_towhcombo.add({
                VALUE: tmpArray[0],
                TEXT: tmpArray[1],
                COMBITEM: tmpArray[0] + tmpArray[1]
            });
            f.findField("TOWH").setValue(tmpArray[0]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("APPLY_NOTE").setValue(T1LastRec.data["APPLY_NOTE"]);
            f.findField("FLOWID").setValue(T1LastRec.data["FLOWID"]);
            tmpArray = T1LastRec.data["MAT_CLASS"].split(" ");
            f.findField('MAT_CLASS').setValue(tmpArray[0]);

            f.findField('FRWH').setReadOnly(true);
            f.findField('TOWH').setReadOnly(true);
            f.findField('MAT_CLASS').setReadOnly(true);
            f.findField('FLOWID').setVisible(true);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);

            if (T1Grid.getSelection().length == 1) {
                if (T1LastRec.data["FLOWID"].split(' ')[0].slice(-1) == '2') // 點收中,繳回中(尾碼為2)才可以列印
                    T1Grid.down('#print').setDisabled(false);
                else
                    T1Grid.down('#print').setDisabled(true);
            }
            else
                T1Grid.down('#print').setDisabled(true);
        }
    }
    //T1按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        Tabs.setActiveTab('Form');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        var f = T1Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCM'); // /Scripts/app/model/MiUnitexch.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("FRWH");
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField('FRWH').setReadOnly(false);
            f.findField('TOWH').setReadOnly(true);
            //if (st_frwhcombo.data.length > 0) {
            //    f.findField('FRWH').setValue(st_frwhcombo.getAt(0).get('VALUE'));
            //}
            f.findField("DOCNO").setValue('系統自編');
            f.findField("APPID").setValue(userId + ' ' + userName);
            f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);
            f.findField("APPTIME").setValue(getChtToday());
        }
        else {
            u = f.findField('MAT_CLASS');
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("APPLY_NOTE").setValue(T1LastRec.data["APPLY_NOTE"]);
        }
        f.findField('x').setValue(x);
        f.findField('APPLY_NOTE').setReadOnly(false);
        f.findField('FLOWID').setVisible(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
    }
    //T1按列印
    function showReport(P0) {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl
                + '?P0=' + P0
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
    //T1按儲存
    function T1Submit() {
        var f = T1Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T1Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T1Query.getForm().reset();
                            var v = action.result.etts[0];
                            T2Store.removeAll();
                            T1Query.getForm().findField('P1').setValue(v.DOCNO);
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                        case "R":
                            msglabel('訊息區:資料退回成功');
                            T1Set = '';
                            break;
                    }

                    T1Cleanup();
                    T1Load();
                    Tabs.setActiveTab('Query');
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T1Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();

        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        f.findField('MAT_CLASS').setReadOnly(true);
        f.findField('APPLY_NOTE').setReadOnly(true);
        f.findField('FRWH').setReadOnly(true);
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        T1Query.getForm().findField('P2').setValue(arrP2);
    }


    //定義有多少欄位參數
    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },      // 繳回單號
            { name: 'SEQ', type: 'string' },        // 項次
            { name: 'MMCODE', type: 'string' },     // 院內碼
            { name: 'APVQTY', type: 'string' },     // 繳回數量
            { name: 'MMNAME_C', type: 'string' },   // 中文品名
            { name: 'MMNAME_E', type: 'string' },   // 英文品名
            { name: 'EXP_DATE', type: 'string' },   //效期
            { name: 'LOT_NO', type: 'string' },     //批號
            { name: 'BASE_UNIT', type: 'string' },  // 計量單位
            { name: 'DISC_UPRICE', type: 'string' },// 優惠最小單價
            { name: 'LAST_QTY', type: 'string' },
            { name: 'INV_QTY', type: 'string' },    // 出庫庫房存量
            { name: 'FLOWID', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 1000,
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST'
            },
            url: '/api/AB0112/AllMeDocexp',
            reader: {
                type: 'json',
                rootProperty: 'etts'
                //totalProperty: 'rc'
            }
        }
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增',
                id: 'btnAdd2',
                name: 'btnAdd2',
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AB0112/DetailCreate';
                    setFormT2('I', '新增');
                }
            },
            {
                text: '修改',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AB0112/DetailUpdate';
                    setFormT2('U', '修改');
                }
            },
            {
                text: '刪除',
                id: 'btnDel2',
                name: 'btnDel2',
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length) {
                        let docno = '';
                        let seq = '';

                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        });

                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0112/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            T2Grid.getSelectionModel().deselectAll();

                                            Ext.getCmp('btnUpdate2').setDisabled(true);
                                            Ext.getCmp('btnDel2').setDisabled(true);

                                            T2Cleanup();
                                            T2Load();
                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    }
                                });
                            }
                        });
                    }
                }
            }
        ]
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [
            {
                dock: 'top',
                xtype: 'toolbar',
                items: [T2Tool]
            }
        ],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "繳回單號",
                dataIndex: 'DOCNO',
                width: 150
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "繳回數量",
                dataIndex: 'APVQTY',
                width: 100,
                align: 'right'
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 300
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 300
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70
            },
            {
                text: "效期",
                dataIndex: 'EXP_DATE',
                width: 70
            },
            {
                text: "批號",
                dataIndex: 'BASE_UNIT',
                width: 70
            },
            {
                text: "繳回後剩餘存量",
                dataIndex: 'LAST_QTY',
                width: 130,
                align: 'right'
            },
            {
                text: "出庫庫房存量",
                dataIndex: 'INV_QTY',
                width: 130,
                align: 'right'
            },
            {
                text: "備註",
                dataIndex: 'ITEM_NOTE',
                width: 150
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    if (T2Form.hidden === true) {
                        T1Form.setVisible(false);
                        T2Form.setVisible(true);
                    }
                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null) {
                        Ext.getCmp('btnUpdate2').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(true);

                        var tmpArray = T1LastRec.data["FLOWID"].split(' ');

                        // 修改：單筆主檔狀態為申請中才可修改
                        if (tmpArray[0] === '1' || tmpArray[0] === '0401') {    // 1-衛材申請中, 0401-藥材申請中
                            if (T2Rec.length > 0) {
                                // detail有勾選資料就開放刪除按鈕 不論單選多選
                                Ext.getCmp('btnDel2').setDisabled(false);

                                // detail要單選才開放修改按鈕
                                if (T2Rec.length == 1) {
                                    Ext.getCmp('btnUpdate2').setDisabled(false);
                                }
                                else {
                                    Ext.getCmp('btnUpdate2').setDisabled(true);
                                }
                            }
                            else {
                                Ext.getCmp('btnDel2').setDisabled(true);
                            }
                        }
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records;
                T2LastRec = records[records.length - 1];
                setFormT2a();
            },
            deselect: function (model, record, index) {
                Ext.getCmp('btnUpdate2').setDisabled(true);
                Ext.getCmp('btnDel2').setDisabled(true);
            }
        }
    });
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.load({
                params: {
                    start: 0
                }
            });
        }
    }

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'vbox',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: 60,
            width: 300
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '繳回單號',
                name: 'DOCNO2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                fieldLabel: '項次',
                name: 'SEQ',
                xtype: 'hidden',
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '物料分類',
                name: 'MAT_CLASS2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true,
                hidden: true
            },
            {
                xtype: 'container',
                layout: 'hbox',
                padding: '0 7 7 0',
                items: [
                    mmCodeCombo,
                    {
                        xtype: 'button',
                        itemId: 'btnMmcode',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T2Form.getForm();
                            if (!f.findField("MMCODE").readOnly) {
                                popMmcodeForm_14(viewport, '/api/AB0112/GetMmcode', {
                                    DOCNO: f.findField("DOCNO2").getValue(),
                                    MMCODE: f.findField("MMCODE").getValue()
                                }, setMmcode);
                            }
                        }
                    }
                ]
            },
            {
                xtype: 'displayfield',
                fieldLabel: '中文品名',
                name: 'MMNAME_C',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '英文品名',
                name: 'MMNAME_E',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            }, {
                xtype: 'container',
                layout: 'hbox',
                padding: '0 7 7 0',
                items: [
                    //{
                    //    xtype: 'hidden',
                    //    name: 'LOT_NO_N'
                    //},
                    {
                        xtype: 'textfield',
                        fieldLabel: '批號',
                        name: 'LOT_NO',
                        allowBlank: false,
                        enforceMaxLength: true,
                        maxLength: 20,
                        fieldCls: 'required',
                        readOnly: true
                    },
                    {
                        xtype: 'button',
                        itemId: 'btnLotno',
                        iconCls: 'TRASearch',
                        handler: function () {
                            var f = T2Form.getForm();
                            if (!f.findField("LOT_NO").readOnly) {
                                popLotnoForm(viewport, '/api/AB0112/GetLotno', {
                                    DOCNO: f.findField("DOCNO2").getValue(),
                                    MMCODE: f.findField("MMCODE").getValue()
                                }, setLotno);
                            }
                        }
                    }

                ]
            },
            {
                xtype: 'datefield',
                fieldLabel: '效期',
                name: 'EXP_DATE',
                fieldCls: 'required',
                allowBlank: false, // 欄位為必填
                readOnly: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '計量單位',
                name: 'BASE_UNIT',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textfield',
                fieldLabel: '繳回數量',
                name: 'APVQTY',
                enforceMaxLength: true,
                maxLength: 8,
                submitValue: true,
                allowBlank: false,
                fieldCls: 'required'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '現有數量',
                name: 'INV_QTY',
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textfield',
                fieldLabel: '備註',
                name: 'ITEM_NOTE',
                enforceMaxLength: true,
                maxLength: 8,
                submitValue: true
            }
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().isValid()) {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('提醒', '輸入資料格式有誤');
                }
            },
            {
                itemId: 'cancel', text: '取消', hidden: true, handler: T2Cleanup
            }
        ]
    });
    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T1LastRec != null && T2LastRec != null) {
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('LOT_NO').setValue(T2LastRec.data["LOT_NO"]);
            f.findField('EXP_DATE').setValue(T2LastRec.data["EXP_DATE"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APVQTY').setValue(T2LastRec.data["APVQTY"]);
            f.findField('INV_QTY').setValue(T2LastRec.data["INV_QTY"]);
            f.findField("ITEM_NOTE").setValue(T2LastRec.data["ITEM_NOTE"]);
            f.findField('MMCODE').setReadOnly(true);
            f.findField('APVQTY').setReadOnly(true);
            f.findField('LOT_NO').setReadOnly(true);

            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);
        }

    }
    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        var f = T2Form.getForm();
        if (x === "I") {//新增
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCEXP'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField("DOCNO2").setValue(T1LastRec.data["DOCNO"]);
            f.findField("MAT_CLASS2").setValue(T1LastRec.data["MAT_CLASS"]);

        }
        else {//修改
            u = f.findField('MMCODE');
            u.setReadOnly(true);

            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("MAT_CLASS2").setValue(T1LastRec.data["MAT_CLASS"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('LOT_NO').setValue(T2LastRec.data["LOT_NO"]);
            f.findField('EXP_DATE').setValue(T2LastRec.data["EXP_DATE"]);
            f.findField('APVQTY').setValue(T2LastRec.data["APVQTY"]);
            f.findField('SEQ').setValue(T2LastRec.data["SEQ"]);
            f.findField('INV_QTY').setValue(T2LastRec.data["INV_QTY"]);
        }
        f.findField('x').setValue(x);
        f.findField('LOT_NO').setReadOnly(false);
        f.findField('EXP_DATE').setReadOnly(false);
        f.findField('APVQTY').setReadOnly(false);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        u.focus();
    }
    //按下儲存
    function T2Submit() {
        var f = T2Form.getForm();
        if (f.isValid()) {
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            f.submit({
                url: T1Set,
                success: function (form, action) {
                    myMask.hide();
                    var f2 = T2Form.getForm();
                    var r = f2.getRecord();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            T2Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';

                            // 效期：明細內有批號效期管制品項才enable
                            Ext.Ajax.request({
                                url: '/api/AB0112/CheckExp',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1LastRec.data["DOCNO"]
                                },
                                success: function (response) {
                                    //Ext.getCmp('btnExp').setDisabled(false);
                                },
                                failure: function (response) {
                                    Ext.getCmp('btnExp').setDisabled(true);
                                }
                            });

                            break;
                        case "U":
                            T2Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                    }

                    T2Cleanup();
                },
                failure: function (form, action) {
                    myMask.hide();
                    switch (action.failureType) {
                        case Ext.form.action.Action.CLIENT_INVALID:
                            Ext.Msg.alert('失敗', 'Form fields may not be submitted with invalid values');
                            break;
                        case Ext.form.action.Action.CONNECT_FAILURE:
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                            break;
                        case Ext.form.action.Action.SERVER_INVALID:
                            Ext.Msg.alert('失敗', action.result.msg);
                            break;
                    }
                }
            });
        }
    }
    function T2Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
    }



    var Tabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        layout: 'fit',
        defaults: {
            layout: 'fit'
        },
        items: [{
            itemId: 'Query',
            title: '查詢',
            items: [T1Query]
        }, {
            itemId: 'Form',
            title: '瀏覽',
            items: [T1Form, T2Form]
        }]
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
            },
            {
                itemId: 'form',
                id: 'eastform',
                region: 'east',
                collapsible: true,
                floatable: true,
                width: 350,
                title: '瀏覽',
                border: false,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [Tabs]
            }
        ]
    });

    MatClassLoad();

    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnSubmitC').setDisabled(true);
    Ext.getCmp('btnExp').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);
});