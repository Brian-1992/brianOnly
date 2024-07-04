Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "非庫備核撥作業";
    var reportUrl = '/Report/A/AA0013.aspx';

    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var arrP4 = ["2", "4"];
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0157/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_apply_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0157/GetApplyKindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_Flowid = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0157/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_towh = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0157/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts' 
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, records, successful, eOpts) {
                store.insert(0, { TEXT: '', VALUE: '' });
                T1QueryForm.getForm().findField('P5').setValue('');
            }
        }
    });

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    // 查詢欄位
    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
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
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P0',
                id: 'P0',
                store: st_matclass,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'P1',
                id: 'P1',
                vtype: 'dateRange',
                dateRange: { end: 'P2' },
                padding: '0 4 0 4',
                value: getToday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'P2',
                id: 'P2',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'P1' },
                padding: '0 4 0 4'
            }, {
                xtype: 'combo',
                fieldLabel: '申請單分類',
                name: 'P3',
                id: 'P3',
                store: st_apply_kind,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '申請單狀態',
                name: 'P4',
                id: 'P4',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '入庫庫房',
                name: 'P5',
                id: 'P5',
                store: st_towh,
                queryMode: 'local',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'datefield',
                fieldLabel: '撥發日期',
                name: 'P6',
                id: 'P6',
                padding: '0 4 0 4',
                value: getToday()
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: function () {
                T1Load();
                msglabel('訊息區:');
            }
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                msglabel('訊息區:');
                T1QueryForm.getForm().findField('P4').setValue(arrP4);
            }
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'DOCTYPE', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'APPID', type: 'string' },
            { name: 'APPDEPT', type: 'string' },
            { name: 'APPTIME', type: 'string' },
            { name: 'USEID', type: 'string' },
            { name: 'USEDEPT', type: 'string' },
            { name: 'FRWH', type: 'string' },
            { name: 'TOWH', type: 'string' },
            { name: 'PURCHASENO', type: 'string' },
            { name: 'SUPPLYNO', type: 'string' },
            { name: 'APPLY_KIND', type: 'string' },
            { name: 'MAT_CLASS', type: 'string' },
            { name: 'APPLY_NOTE', type: 'string' },
            { name: 'CREATE_DATE', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MAT_CLASS_N', type: 'string' },
            { name: 'FRWH_N', type: 'string' },
            { name: 'TOWH_N', type: 'string' },
            { name: 'APPTIME_T', type: 'string' },
            { name: 'APP_NAME', type: 'string' },
            { name: 'EXT', type: 'string' },
            { name: 'LIST_ID', type: 'string' },
            { name: 'APPLY_DATE', type: 'string' }, 
            { name: 'SRCDOCYN', type: 'string' },
            { name: 'WH_GRADE', type: 'string' },
            { name: 'RDOCNO', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 5, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0157/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        },
        listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').rawValue,
                    p2: T1QueryForm.getForm().findField('P2').rawValue,
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    p4: T1QueryForm.getForm().findField('P4').getValue(),
                    p5: T1QueryForm.getForm().findField('P5').getValue(),
                    p6: T1QueryForm.getForm().findField('P6').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Grid.down('#apply').setDisabled(true);
        T1Grid.down('#applyx').setDisabled(true);

        Ext.getCmp('btnUpdate2').setDisabled(true);
        Ext.getCmp('btnCancel2').setDisabled(true);
        Ext.getCmp('btnTransConfirm').setDisabled(true);
        Ext.getCmp('btnTransCancel').setDisabled(true);

        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'apply', text: '核撥', disabled: true, handler: function () {
                    // 檢查是否有尚未儲存的明細資料
                    var tempData = T2Grid.getStore().data.items;
                    var isdirty = false;
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            isdirty = true;
                        }
                    }
                    if (isdirty) {
                        Ext.Msg.alert('提醒', '尚有資料未儲存');
                    }
                    else {
                        var selection = T1Grid.getSelection();
                        if (selection.length === 0) {
                            Ext.Msg.alert('提醒', '請勾選項目');
                        }
                        else {
                            let name = '';
                            let docno = '';
                            //selection.map(item => {
                            //    name += '「' + item.get('DOCNO') + '」<br>';
                            //    docno += item.get('DOCNO') + ',';
                            //});
                            $.map(selection, function (item, key) {
                                name += '「' + item.get('DOCNO') + '」<br>';
                                docno += item.get('DOCNO') + ',';
                            })
                            Ext.MessageBox.confirm('核撥', '是否確定核撥？單號如下<br>' + name, function (btn, text) {
                                if (btn === 'yes') {
                                    var data = [];
                                    for (var i = 0; i < selection.length; i++) {
                                        data.push(selection[i].data);
                                    }

                                    // 檢查所選申請單內是否有註記轉申購單品項
                                    Ext.Ajax.request({
                                        url: '/api/AA0157/ChkIsTransPr',
                                        method: reqVal_p,
                                        contentType: "application/json",
                                        params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                                        success: function (response) {
                                            var rdata = Ext.decode(response.responseText);
                                            if (rdata.success) {
                                                if (rdata.msg == 'Y') {
                                                    Ext.MessageBox.confirm('核撥', '註記轉申購單品項將設定核撥量為0過帳，並產生新單據，請確認是否核撥?', function (btn, text) {
                                                        if (btn === 'yes') {
                                                            chkAppqty(data);
                                                        }
                                                    }
                                                    );
                                                }
                                                else
                                                    chkAppqty(data);
                                            }
                                            else
                                                Ext.MessageBox.alert('錯誤', rdata.msg);
                                        },

                                        failure: function (response, action) {
                                            myMask.hide();
                                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                                        }
                                    });
                                }
                            }
                            );
                        }
                    }
                }
            }, {
                itemId: 'applyx', text: '退回', disabled: true, handler: function () {
                    var tempData = T2Grid.getStore().data.items;
                    var isdirty = false;
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            isdirty = true;
                        }
                    }
                    if (isdirty) {
                        Ext.Msg.alert('提醒', '尚有資料未儲存');
                    }
                    else {
                        var selection = T1Grid.getSelection();
                        if (selection.length === 0) {
                            Ext.Msg.alert('提醒', '請勾選項目');
                        }
                        else {
                            let name = '';
                            let docno = '';
                            //selection.map(item => {
                            //    name += '「' + item.get('DOCNO') + '」<br>';
                            //    docno += item.get('DOCNO') + ',';
                            //});
                            $.map(selection, function (item, key) {
                                name += '「' + item.get('DOCNO') + '」<br>';
                                docno += item.get('DOCNO') + ',';
                            })
                            Ext.MessageBox.confirm('退回', '是否確定退回？單號如下<br>' + name, function (btn, text) {
                                if (btn === 'yes') {
                                    Ext.Ajax.request({
                                        url: '/api/AA0157/ApplyX',
                                        method: reqVal_p,
                                        params: {
                                            DOCNO: docno
                                        },
                                        //async: true,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                T2Store.removeAll();
                                                T1Grid.getSelectionModel().deselectAll();
                                                T1Load();
                                                msglabel('訊息區:退回成功');
                                            }
                                            else
                                                Ext.MessageBox.alert('錯誤', data.msg);
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                }
                            }
                            );
                        }
                    }
                }
            },
            {
                itemId: 'print', text: '列印', disabled: true, handler: function () {
                    PrintReport();
                }
            }
        ]
    });

    // 檢查所選申請單內是否有(預計撥發量+調撥量)小於申請量且未註記轉申購單品項
    function chkAppqty(data) {
        Ext.Ajax.request({
            url: '/api/AA0157/chkAppqty',
            method: reqVal_p,
            contentType: "application/json",
            params: { ITEM_STRING: Ext.util.JSON.encode(data) },
            success: function (response) {
                var rdata = Ext.decode(response.responseText);
                if (rdata.success) {
                    if (rdata.msg != '') {
                        var rtnStr = rdata.msg.split('^');
                        Ext.MessageBox.confirm('核撥', rtnStr[0] + '該申請單內(院內碼:' + rtnStr[1] + ')預計撥發量(含調撥量)不足申請量且未註記轉訂單之品項，其核撥後將不列入申購彙總，請確認是否核撥?', function (btn, text) {
                            if (btn === 'yes') {
                                ApplyDoc(data);
                            }
                        }
                        );
                    }
                    else
                        ApplyDoc(data);
                }
                else
                    Ext.MessageBox.alert('錯誤', rdata.msg);
            },

            failure: function (response, action) {
                myMask.hide();
                Ext.Msg.alert('失敗', 'Ajax communication failed');
            }
        });
    }

    function ApplyDoc(data) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();

        Ext.Ajax.request({
            url: '/api/AA0157/Apply',
            method: reqVal_p,
            params: { ITEM_STRING: Ext.util.JSON.encode(data) },
            //async: true,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    msglabel('訊息區:核撥成功');
                }
                else
                    Ext.MessageBox.alert('錯誤', data.msg);
            },
            failure: function (response) {
                myMask.hide();
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }

    // 查詢結果資料列表
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        selModel: {
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI'
        },
        selType: 'checkboxmodel',
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "申請單號",
            dataIndex: 'DOCNO',
            width: 150
        }, {
            text: "類別",
            dataIndex: 'APPLY_KIND_N',
            width: 80
        }, {
            text: "狀態",
            dataIndex: 'FLOWID_N',
            width: 80
        }, {
            text: "申請日期",
            dataIndex: 'APPTIME_T',
            width: 100
        }, {
            text: "出庫庫房",
            dataIndex: 'FRWH_N',
            width: 120
        }, {
            text: "入庫庫房",
            dataIndex: 'TOWH_N',
            width: 120
        }, {
            text: "物料分類",
            dataIndex: 'MAT_CLASS_N',
            width: 100
        }, {
            text: "申請人員",
            dataIndex: 'APP_NAME',
            width: 80
        }, {
            text: "分機號碼",
            dataIndex: 'EXT',
            width: 100
        }, {
            text: "轉申購",
            dataIndex: 'SRCDOCYN',
            width: 100
        }, {
            text: "申購單號",
            dataIndex: 'RDOCNO',
            width: 100
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    chkBtnStatus();
                }
            },
            selectionchange: function (model, records) {
                var tempData = T2Grid.getStore().data.items;
                var isdirty = false;
                for (var i = 0; i < tempData.length; i++) {
                    if (tempData[i].dirty) {
                        isdirty = true;
                    }
                }
                if (isdirty) {
                    Ext.Msg.alert('提醒', '尚有資料未儲存');
                }
                else {
                    T1Rec = records.length;
                    T1LastRec = records[0];
                    setFormT1a();
                }
            }
        }
    });

    function chkBtnStatus() {
        if (T1LastRec != null) {
            var canEdit = true;
            var canTrans = true;
            var canSubmit = true;
            var selection = T1Grid.getSelection();
            if (selection.length) {
                if (selection.length > 1) {
                    // 勾選資料大於1筆則不可使用明細功能鈕
                    canEdit = false;
                    canTrans = false;
                }
                else {
                    // frwh的wh_grade非一級庫則不可設定轉申購
                    if (selection[0].get('WH_GRADE') != '1') {
                        canTrans = false;
                    }
                }

                $.map(selection, function (item, key) {
                    // 有任一筆不是核撥中或申購單號不為空白則不可核撥和退回
                    if (item.get('FLOWID').slice(-1) != '2' || item.get('RDOCNO') != '') {
                        canEdit = false;
                        canSubmit = false;
                        canTrans = false;
                    }
                })
            }
            if (canEdit) {
                Ext.getCmp('btnUpdate2').setDisabled(false);
                Ext.getCmp('btnCancel2').setDisabled(false);
                Ext.getCmp('btnTransCancel').setDisabled(false);
            }
            else {
                Ext.getCmp('btnUpdate2').setDisabled(true);
                Ext.getCmp('btnCancel2').setDisabled(true);
                Ext.getCmp('btnTransCancel').setDisabled(true);
            }

            if (canTrans)
                Ext.getCmp('btnTransConfirm').setDisabled(false);
            else
                Ext.getCmp('btnTransConfirm').setDisabled(true);

            if (canSubmit) {
                T1Grid.down('#apply').setDisabled(false);
                T1Grid.down('#applyx').setDisabled(false);
            }
            else {
                T1Grid.down('#apply').setDisabled(true);
                T1Grid.down('#applyx').setDisabled(true);
            }
        }
        else {
            T1Grid.down('#apply').setDisabled(true);
            T1Grid.down('#applyx').setDisabled(true);

            Ext.getCmp('btnUpdate2').setDisabled(true);
            Ext.getCmp('btnCancel2').setDisabled(true);
            Ext.getCmp('btnTransConfirm').setDisabled(true);
            Ext.getCmp('btnTransCancel').setDisabled(true);
        }
    }

    function setFormT1a() {
        chkBtnStatus();
        T1Grid.down('#print').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            T1F1 = T1LastRec.data['DOCNO'];
            T1F3 = T1LastRec.data['FLOWID'];
        }
        else {
            T1F1 = '';
        }
        T2Query.getForm().reset();
        T2Load();
    }

    //Detail
    var T2Rec = 0;
    var T2LastRec = null;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0157/GetMMCodeDocd',
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1LastRec.data['DOCNO']
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 50
        },
        border: false,
        items: [
            T2QueryMMCode, {
                xtype: 'button',
                text: '查詢',
                handler: T2Load
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus();
                }
            }]
    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'DOCNO', type: 'string' },
            { name: 'SEQ', type: 'string' },
            { name: 'MMCODE', type: 'string' },
            { name: 'APPQTY', type: 'string' },
            { name: 'APVQTY', type: 'string' },
            { name: 'APVTIME', type: 'string' },
            { name: 'APVID', type: 'string' },
            { name: 'ACKQTY', type: 'string' },
            { name: 'ACKID', type: 'string' },
            { name: 'ACKTIME', type: 'string' },
            { name: 'STAT', type: 'string' },
            { name: 'RSEQ', type: 'string' },
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'BW_MQTY', type: 'string' },
            { name: 'PICK_QTY', type: 'string' },
            { name: 'PICK_USER', type: 'string' },
            { name: 'PICK_TIME', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'CREATE_USER', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMCODE_N', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'M_CONTPRICE', type: 'string' },
            { name: 'AVG_PRICE', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'AVG_APLQTY', type: 'string' },
            { name: 'SAFE_QTY', type: 'string' },
            { name: 'TOT_APVQTY', type: 'string' },
            { name: 'TOT_BWQTY', type: 'string' },
            { name: 'TOT_DISTUN', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'HIGH_QTY', type: 'string' },
            { name: 'SGN', type: 'string' },
            { name: 'A_INV_QTY', type: 'string' },
            { name: 'B_INV_QTY', type: 'string' },
            { name: 'GTAPL_RESON', type: 'string' },
            { name: 'GTAPL_RESON_N', type: 'string' },
            { name: 'DIS_TIME_T', type: 'string' },
            { name: 'CAN_DIST_QTY', type: 'string' },
            { name: 'APL_CONTIME', type: 'string' },
            { name: 'ISTRANSPR', type: 'string' },
            { name: 'SHORT_REASON', type: 'string' }
        ]
    });
    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0157/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            },
            timeout: 0
        },
        listeners: {
            beforeload: function (store, options) {
                store.removeAll();
                var np = {
                    p0: T1F1,
                    p1: T2Query.getForm().findField('P1').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T2Load() {
        try {
            T2Tool.moveFirst();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '更新',
                id: 'btnUpdate2',
                name: 'btnUpdate2',
                disabled: true,
                handler: function () {
                    var tempData = T2Grid.getStore().data.items;

                    var data = [];
                    for (var i = 0; i < tempData.length; i++) {
                        if (tempData[i].dirty) {
                            data.push(tempData[i].data);
                        }
                    }
                    var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                    myMask.show();

                    Ext.Ajax.request({
                        url: '/api/AA0157/UpdateMeDocd',
                        method: reqVal_p,
                        contentType: "application/json",
                        params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                        success: function (response) {

                            myMask.hide();
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('訊息區:資料修改成功');
                                T2Tool.moveFirst();
                            }
                            else
                                Ext.MessageBox.alert('錯誤', data.msg);
                        },

                        failure: function (response, action) {
                            myMask.hide();
                            Ext.Msg.alert('失敗', 'Ajax communication failed');
                        }
                    });
                }
            }, {
                text: '取消',
                id: 'btnCancel2',
                name: 'btnCancel2',
                disabled: true,
                handler: function () {
                    T2Store.load({
                        params: {
                            start: 0
                        }
                    });
                }
            }, {
                text: '設定轉申購',
                id: 'btnTransConfirm',
                name: 'btnTransConfirm',
                disabled: true,
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length > 0) {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();

                        var selData = [];
                        for (var i = 0; i < selection.length; i++) {
                            selData.push(selection[i].data);
                        }

                        Ext.Ajax.request({
                            url: '/api/AA0157/TransConfirmMeDocd',
                            method: reqVal_p,
                            contentType: "application/json",
                            params: { ITEM_STRING: Ext.util.JSON.encode(selData) },
                            success: function (response) {
                                myMask.hide();

                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    msglabel('訊息區:設定轉申購成功');
                                    T2Tool.moveFirst();
                                }
                                else
                                    Ext.MessageBox.alert('錯誤', data.msg);
                            },

                            failure: function (response, action) {
                                myMask.hide();
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('訊息', '請先勾選要設定轉申購的院內碼');
                }
            }, {
                text: '取消設定轉申購',
                id: 'btnTransCancel',
                name: 'btnTransCancel',
                disabled: true,
                handler: function () {
                    var selection = T2Grid.getSelection();
                    if (selection.length > 0) {
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();

                        var selData = [];
                        for (var i = 0; i < selection.length; i++) {
                            selData.push(selection[i].data);
                        }

                        Ext.Ajax.request({
                            url: '/api/AA0157/TransCancelMeDocd',
                            method: reqVal_p,
                            contentType: "application/json",
                            params: { ITEM_STRING: Ext.util.JSON.encode(selData) },
                            success: function (response) {
                                myMask.hide();
                                var data = Ext.decode(response.responseText);
                                if (data.success) {
                                    msglabel('訊息區:取消設定轉申購成功');
                                    T2Tool.moveFirst();
                                }
                                else
                                    Ext.MessageBox.alert('錯誤', data.msg);
                            },

                            failure: function (response, action) {
                                myMask.hide();
                                Ext.Msg.alert('失敗', 'Ajax communication failed');
                            }
                        });
                    }
                    else
                        Ext.Msg.alert('訊息', '請先勾選要取消設定轉申購的院內碼');
                }
            }
        ]
    });

    var T2LastRecTmp;
    var T2Grid = Ext.create('Ext.grid.Panel', {
        title: '',
        store: T2Store,
        plain: true,
        loadMask: true,
        cls: 'T2',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T2Query]
        },{
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
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "轉申購",
            dataIndex: 'ISTRANSPR',
            width: 70
        }, {
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 80,
            sortable: true
        }, {
            text: "中文品名",
            dataIndex: 'MMNAME_C',
            width: 120,
            sortable: true
        }, {
            text: "英文品名",
            dataIndex: 'MMNAME_E',
            width: 120,
            sortable: true
        }, {
            text: "單位",
            dataIndex: 'BASE_UNIT',
            width: 50,
            sortable: true
        }, {
            text: "民庫存量",
            dataIndex: 'INV_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "囤儲量",
            dataIndex: 'CAN_DIST_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "申請量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 70, align: 'right'
        }, {
            text: "月基準量",
            dataIndex: 'HIGH_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "月累計量",
            dataIndex: 'TOT_APVQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "撥發包裝率",
            dataIndex: 'TOT_DISTUN',
            width: 90
        }, {
            text: "戰備存量",
            dataIndex: 'A_INV_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "累計調撥量",
            dataIndex: 'TOT_BWQTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "建議核撥量",
            dataIndex: 'B_INV_QTY',
            style: 'text-align:left',
            width: 100, align: 'right'
        }, {
            text: "<b><font color=red>預計撥發量</font></b>",
            dataIndex: 'EXPT_DISTQTY',
            style: 'text-align:left',
            width: 100,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            }, align: 'right'
        }, {
            text: "<b><font color=red>調撥量</font></b>",
            dataIndex: 'BW_MQTY',
            style: 'text-align:left',
            width: 80,
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            }, align: 'right'
        }, {
            text: "實際撥發量",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 100,
            align: 'right'
        }, {
            text: "核撥時間",
            dataIndex: 'DIS_TIME_T',
            width: 80
        }, {
            text: "庫存單價",
            dataIndex: 'AVG_PRICE',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "安全存量",
            dataIndex: 'SAFE_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "備註",
            dataIndex: 'APLYITEM_NOTE',
            width: 170,
            maxLength: 50
        }, {
            text: "超量原因",
            dataIndex: 'GTAPL_RESON_N',
            width: 180
        }, {
            text: "送核撥時間",
            dataIndex: 'APL_CONTIME',
            width: 110
        }, {
            header: "",
            flex: 1
        }
        ],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                        // 編輯前紀錄要用到的值(使用者採用點其他列的資料完成編輯會導致T1LastRec被更新)
                        if (T2LastRec != null) {
                            T2LastRecTmp = T2LastRec;
                        }
                        if (T1F3 === '2' && T1LastRec.data['RDOCNO'] == '') {
                            return true;
                        }
                        else {
                            return false; // 取消row editing模式
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                        var chkValid = true;
                        var failMsg = '';
                        // 預計撥發量
                        if (context.field == "EXPT_DISTQTY" && context.value != "") {
                            if (Number(context.value) + Number(T2LastRecTmp.data['BW_MQTY']) > Number(T2LastRecTmp.data['APPQTY'])) {
                                chkValid = false;
                                failMsg = '(預計撥發量+調撥量)大於申請量';
                            }
                            //else if (Number(context.value) > Number(T2LastRecTmp.data['B_INV_QTY'])) {
                            //    chkValid = false;
                            //    failMsg = '預計撥發量大於建議核撥量';
                            //}
                        }
                        // 調撥量
                        else if (context.field == "BW_MQTY" && context.value != "") {
                            if (Number(T2LastRecTmp.data['EXPT_DISTQTY']) + Number(context.value) > Number(T2LastRecTmp.data['APPQTY'])) {
                                chkValid = false;
                                failMsg = '(預計撥發量+調撥量)大於申請量';
                            }
                            else if (Number(context.value) > Number(T2LastRecTmp.data['A_INV_QTY'])) {
                                chkValid = false;
                                failMsg = '調撥量大於戰備存量';
                            }
                        }

                        if (chkValid == false) {
                            Ext.MessageBox.alert('錯誤', failMsg);
                            context.cancel = true;
                            context.record.data[context.field] = context.originalValue;
                        }
                    }
                }
            })
        ],
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
            }
        }
    });
    
    function PrintReport() {
        var selection = T1Grid.getSelection();
        var docnos = '';
        for (var i = 0; i < selection.length; i++) {
            if (docnos != '') {
                docnos += ',';
            }
            docnos += selection[i].data.DOCNO;
        }

        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?p5=' + docnos + '&Action=1&Order=storeloc" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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
        this.up('window').destroy();
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
            items: [{
                //  xtype:'container',
                region: 'center',
                layout: {
                    type: 'border',
                    padding: 0
                },
                collapsible: false,
                title: '',
                split: true,
                width: '80%',
                flex: 1,
                minWidth: 50,
                minHeight: 140,
                items: [
                    {
                        region: 'north',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '30%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '70%',
                        split: true,
                        items: [T2Grid]
                    }
                ]
            }]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: 300,
            title: '查詢',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1QueryForm]
        }
        ]
    });

    T1QueryForm.getForm().findField('P0').focus();
    T1QueryForm.getForm().findField('P3').setValue('1');
    T1QueryForm.getForm().findField('P4').setValue(arrP4);
});
