Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "撥發提領";
    var reportUrl = '/Report/A/AA0147.aspx';
    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var arrP3 = [];
    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    function sanitize(value) {
        if (Array.isArray(value)) {
            var tempResult = [];
            for (var i = 0; i < value.length; i++) {
                var temp = value[i];

                temp = temp.replaceAll('|', '');
                temp = temp.replaceAll('&', '');
                temp = temp.replaceAll(';', '');
                temp = temp.replaceAll('$', '');
                temp = temp.replaceAll('%', '');
                temp = temp.replaceAll('@', '');
                temp = temp.replaceAll("'", '');
                temp = temp.replaceAll('"', '');
                temp = temp.replaceAll('\'', '');
                temp = temp.replaceAll('\"', '');
                temp = temp.replaceAll('<', '');
                temp = temp.replaceAll('>', '');
                temp = temp.replaceAll('(', '');
                temp = temp.replaceAll(')', '');
                temp = temp.replaceAll('+', '');
                temp = temp.replaceAll('\r', '');
                temp = temp.replaceAll('\n', '');
                temp = temp.replaceAll(',', '');
                temp = temp.replaceAll('\\', '');

                tempResult.push(temp);
            }
            return tempResult;
        }

        value = value.replaceAll('|', '');
        value = value.replaceAll('&', '');
        value = value.replaceAll(';', '');
        value = value.replaceAll('$', '');
        value = value.replaceAll('%', '');
        value = value.replaceAll('@', '');
        value = value.replaceAll("'", '');
        value = value.replaceAll('"', '');
        value = value.replaceAll('\'', '');
        value = value.replaceAll('\"', '');
        value = value.replaceAll('<', '');
        value = value.replaceAll('>', '');
        value = value.replaceAll('(', '');
        value = value.replaceAll(')', '');
        value = value.replaceAll('+', '');
        value = value.replaceAll('\r', '');
        value = value.replaceAll('\n', '');
        value = value.replaceAll(',', '');
        value = value.replaceAll('\\', '');

        return value;

    }

    var st_Flowid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM', 'EXTRA1']
    });

    var st_apply_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0147/GetApplyKindCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_appdept = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0147/GetAppDeptCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_matclass = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0147/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var st_towhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0147/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    function setFlowidComboData() {
        Ext.Ajax.request({
            url: '/api/AA0147/GetFlowidCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var flowids = data.etts;
                    if (flowids.length > 0) {
                        for (var i = 0; i < flowids.length; i++) {
                            st_Flowid.add(flowids[i]);
                            if (flowids[i].EXTRA1 == 'Y') {
                                arrP3.push(flowids[i].VALUE);
                            }
                        }
                    }
                }
                T1QueryForm.getForm().findField('P3').setValue(arrP3);
            },
            failure: function (response, options) {

            }
        });

    }
    var T1QueryMMCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P7',
        name: 'P7',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0147/GetMMCodeDocd', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1LastRec.data["DOCNO"]
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    setFlowidComboData();
    // 查詢欄位

    function get6monthday() {
        var rtnDay = addMonths(new Date(), -6);
        return rtnDay
    }
    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        listeners: {
            load: function (store, eOpts) {
                hosp_code = store.getAt(0).get('HOSP_CODE');
                // 依取得的HOSP_CODE處理欄位
                if (store.getAt(0).get('HOSP_CODE') == '811') {
                    // 若為811(澎湖)則顯示[庫存足核撥]按鈕
                    T1Grid.down('#applyInv').show();
                }
                else {
                    T1Grid.down('#applyInv').hide();
                }
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0147/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var mLabelWidth = 100;
    var mWidth = 230;
    var T1QueryForm = Ext.widget({
        xtype: 'form',
        layout: 'form',
        //layout: {
        //    type: 'table',
        //    columns: 1
        //},
        frame: false,
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            xtype: 'textfield',
            labelAlign: 'right',
            // msgTarget: 'side',
            labelWidth: mLabelWidth,
            width: mWidth
        },
        items: [
            {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P5',
                id: 'P5',
                store: st_matclass,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'P0',
                id: 'P0',
                vtype: 'dateRange',
                dateRange: { end: 'P1' },
                padding: '0 4 0 4',
                //value: get6monthday()
            }, {
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'P1',
                id: 'P1',
                labelSeparator: '',
                vtype: 'dateRange',
                dateRange: { begin: 'P0' },
                padding: '0 4 0 4',
                value: getToday()
            }, {
                xtype: 'combo',
                fieldLabel: '申請單狀態',
                name: 'P3',
                id: 'P3',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                matchFieldWidth: false,
                listConfig: { width: 180 }
            }, {
                xtype: 'combo',
                fieldLabel: '入庫庫房(請領單位)',
                name: 'P2',
                id: 'P2',
                store: st_appdept,
                queryMode: 'local',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            },
            T1QueryMMCodeCombo,
            {
                xtype: 'textfield',
                fieldLabel: '單據號碼',
                name: "P8",
                id: "P8"
                //}, {
                //    xtype: 'checkboxfield',
                //    fieldLabel: '進貨待補轉單',
                //    name: 'P6',
                //    id: 'P6',
                //    inputValue: '1',
                //    padding: '0 4 0 4'
            }, {
                xtype: 'checkboxfield',
                fieldLabel: '只列出可提撥',
                name: 'P9',
                id: 'P9',
                inputValue: '1',
                padding: '0 4 0 4',
                checked: true
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
                T1QueryForm.getForm().findField('P3').setValue(arrP3);
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
            { name: 'SRCDOCYN', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 100, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'APPTIME', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0147/AllM',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
            beforeload: function (store, options) {
                // 載入前將查詢條件P0值代入參數
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').rawValue,
                    p1: T1QueryForm.getForm().findField('P1').rawValue,
                    p2: T1QueryForm.getForm().findField('P2').getValue(),
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    //p4: T1QueryForm.getForm().findField('P4').getValue(),
                    p5: T1QueryForm.getForm().findField('P5').getValue(),
                    // p6: T1QueryForm.getForm().findField('P6').rawValue,  //進貨待補轉單
                    p7: T1QueryForm.getForm().findField('P7').rawValue,
                    p8: T1QueryForm.getForm().findField('P8').rawValue,
                    p9: T1QueryForm.getForm().findField('P9').rawValue
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });

    function T1Load() {
        T1Store.load({
            params: {
                start: 0
            }
        });

        T1Tool.moveFirst();
        viewport.down('#form').collapse();
    }

    function setSubmit(args) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/AA0147/Apply',
            method: reqVal_p,
            params: {
                DOCNO: args.DOCNO,
                DATA_LIST: Ext.util.JSON.encode(args.DATA_LIST)
            },
            //async: true,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    msglabel('訊息區:撥發成功');
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

    function setSubmitInv(args) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/AA0147/ApplyInv',
            method: reqVal_p,
            params: {
                DOCNO: args.DOCNO,
                DATA_LIST: Ext.util.JSON.encode(args.DATA_LIST)
            },
            //async: true,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    msglabel('訊息區:庫存足核撥成功');
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
    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            //1121023配合國軍需求改採單筆撥發
            itemId: 'apply', text: '撥發', disabled: true, hidden: true, handler: function () {
                var selection = T1Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    let name = '';
                    let docno = '';
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('DOCNO') + '」<br>';
                        docno += item.get('DOCNO') + ',';
                    })

                    // 若有批號效期資料則需維護各批號效期撥發量
                    Ext.Ajax.request({
                        url: '/api/AA0147/ChkExp',
                        method: reqVal_p,
                        params: {
                            DOCNO: docno
                        },
                        //async: true,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == 'Y') {
                                    popExpForm_14(viewport, '/api/AA0147/GetExpList', {
                                        DOCNO: docno,
                                        CHKCOLTYPE: 1
                                    }, '撥發', setSubmit);
                                }
                                else {
                                    Ext.MessageBox.confirm('撥發', '請確認是否撥發？' + '單號如下<br>' + name, function (btn, text) {
                                        if (btn === 'yes') {
                                            setSubmit({
                                                DOCNO: docno,
                                                DATA_LIST: []
                                            });
                                        }
                                    });
                                }
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
        }, {
            itemId: 'print', text: '列印', disabled: true, handler: function () {
                PrintReport();
            }
        }, {
            itemId: 'applyInv', text: '庫存足核撥', disabled: true, handler: function () {
                var selection = T1Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    let name = '';
                    let docno = '';
                    $.map(selection, function (item, key) {
                        name += '「' + item.get('DOCNO') + '」<br>';
                        docno += item.get('DOCNO') + ',';
                    })
                    // 若有批號效期資料則需維護各批號效期撥發量
                    Ext.Ajax.request({
                        url: '/api/AA0147/ChkExp',
                        method: reqVal_p,
                        params: {
                            DOCNO: docno
                        },
                        //async: true,
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == 'Y') {
                                    popExpForm_14(viewport, '/api/AA0147/GetExpList', {
                                        DOCNO: docno,
                                        CHKCOLTYPE: 1
                                    }, '撥發', setSubmitInv);
                                }
                                else {
                                    Ext.MessageBox.confirm('庫存足核撥', '請確認是否執行庫存足核撥？' + '單號如下<br>' + name, function (btn, text) {
                                        if (btn === 'yes') {
                                            setSubmitInv({
                                                DOCNO: docno,
                                                DATA_LIST: []
                                            });
                                        }
                                    });
                                }
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
        }]
    });

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
            text: "單據號碼",
            dataIndex: 'DOCNO',
            width: 170
        }, {
            text: "申請時間",
            dataIndex: 'APPTIME_T',
            width: 100
        }, {
            text: "出庫庫房",
            dataIndex: 'FRWH_N',
            width: 140
        }, {
            text: "入庫庫房(請領單位)",
            dataIndex: 'TOWH_N',
            width: 160
        }, {
            text: "轉申購單",
            dataIndex: 'SRCDOCYN',
            width: 80
        }, {
            text: "單據狀態",
            dataIndex: 'FLOWID_N',
            width: 150
        }, {
            text: "申請人姓名",
            dataIndex: 'APPUNA',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'APPLY_NOTE',
            width: 400
        }, {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1Grid.getSelection().length > 1) {
                    T1Grid.down('#print').setDisabled(true);
                } else {
                    setFormT1a();
                }
            }
        }
    });

    function setFormT1a() {
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T1Grid.down('#print').setDisabled(T1Rec === 0);
        T1Grid.down('#applyInv').setDisabled(T1Rec === 0);
        Ext.getCmp('btnUpdate').setDisabled(T1Rec === 0);   //更新
        Ext.getCmp('btnCancel').setDisabled(T1Rec === 0);    //取消
        Ext.getCmp('btnDetailApply').setDisabled(T1Rec === 0);
        Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            T1F1 = T1LastRec.data["DOCNO"];
            if ((T1LastRec.data["DOCTYPE"] == "MR") || (T1LastRec.data["DOCTYPE"] == "MS")) {
                T1F3 = right(T1LastRec.data["FLOWID"], 2);
            } else {
                T1F3 = T1LastRec.data["FLOWID"];
            }
            if ((T1F3 == '2') || (T1F3 == '02') || (T1F3 == '4') || (T1F3 == '03')) {
                T1Grid.down('#apply').setDisabled(false);
                T1Grid.down('#applyInv').setDisabled(false);
                Ext.getCmp('btnUpdate').setDisabled(false);   //更新
                Ext.getCmp('btnCancel').setDisabled(false);    //取消
                Ext.getCmp('btnDetailApply').setDisabled(false);
                Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(false);
            }
            else {
                T1Grid.down('#apply').setDisabled(true);
                T1Grid.down('#applyInv').setDisabled(true);
                Ext.getCmp('btnUpdate').setDisabled(true);   //更新
                Ext.getCmp('btnCancel').setDisabled(true);    //取消
                Ext.getCmp('btnDetailApply').setDisabled(true);
                Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(true);
            }
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
        queryUrl: '/api/AA0147/GetMMCodeDocd', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {
                p1: T1LastRec.data["DOCNO"]
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
            labelWidth: 90
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
            { name: 'EXPT_DISTQTY', type: 'string' },
            { name: 'APLYITEM_NOTE', type: 'string' },
            { name: 'UPDATE_DATE', type: 'string' },
            { name: 'UPDATE_USER', type: 'string' },
            { name: 'UPDATE_IP', type: 'string' },
            { name: 'MMNAME_C', type: 'string' },
            { name: 'MMNAME_E', type: 'string' },
            { name: 'BASE_UNIT', type: 'string' },
            { name: 'INV_QTY', type: 'string' },
            { name: 'FLOWID', type: 'string' },
            { name: 'DIS_TIME_T', type: 'string' },
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'ISTRANSPR', type: 'string' },
            { name: 'SHORT_REASON', type: 'string' },
            { name: 'CHINNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' },
            { name: 'POSTID', type: 'string' },
            { name: 'DIS_USER', type: 'string' },
            { name: 'DIS_TIME', type: 'string' }
        ]
    });

    var T2Store = Ext.create('Ext.data.Store', {
        model: 'T2Model',
        pageSize: 10000, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'SEQ', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0147/AllD',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }
        , listeners: {
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
            T2Store.load({
                params: {
                    start: 0
                }
            });
            T2Tool.moveFirst();
        }
        catch (e) {
            alert("T2Load Error:" + e);
        }
        //viewport.down('#form').collapse();
    }
    //T1Load();



    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
            text: '更新',
            id: 'btnUpdate',
            name: 'btnUpdate',
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
                    url: '/api/AA0147/UpdateMeDocd',
                    method: reqVal_p,
                    contentType: "application/json",
                    params: { ITEM_STRING: Ext.util.JSON.encode(data) },
                    success: function (response) {
                        myMask.hide();
                        var data = Ext.decode(response.responseText);
                        if (data.success == true) {
                            msglabel('訊息區:資料修改成功');
                            T2Tool.moveFirst();
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
        }, {
            text: '取消',
            id: 'btnCancel',
            name: 'btnCancel',
            disabled: true,
            handler: function () {
                T2Store.load({
                    params: {
                        start: 0
                    }
                });
            }
        }, {
            text: '單筆撥發',
            id: 'btnDetailApply',
            name: 'btnDetailApply',
            disabled: true,
            handler: function () {
                var selection = T2Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    var seq = '';
                    var docno = '';
                    var name = '';

                    $.map(selection, function (item, key) {
                        seq += item.get('SEQ') + ',';
                        docno += item.get('DOCNO') + ',';
                        name += '「' + item.get('DOCNO') + '+' + item.get('MMCODE') + '」<br>';
                    })
                    //檢查核撥量
                    Ext.Ajax.request({
                        url: '/api/AA0147/ChkApvQtyDetail',
                        method: reqVal_p,
                        params: {
                            DOCNO: docno,
                            SEQ: seq
                        },
                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                if (data.msg == 'Y') {
                                    Ext.MessageBox.alert('提醒', '所勾選單號+院內碼之核撥量不得小於等於0，請檢查');
                                } else {
                                    // 若有批號效期資料則需維護各批號效期撥發量
                                    Ext.Ajax.request({
                                        url: '/api/AA0147/ChkExpDetail',
                                        method: reqVal_p,
                                        params: {
                                            DOCNO: docno,
                                            SEQ: seq
                                        },
                                        //async: true,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                if (data.msg == 'Y') {
                                                    popExpForm_14(viewport, '/api/AA0147/GetExpListDetail', {
                                                        DOCNO: docno,
                                                        SEQ: seq,
                                                        CHKCOLTYPE: 1
                                                    }, '單筆撥發', setSubmitDetail);
                                                }
                                                else {
                                                    Ext.MessageBox.confirm('單筆撥發', '請確認是否單筆撥發？' + '單號+院內碼 如下<br>' + name, function (btn, text) {
                                                        if (btn === 'yes') {
                                                            setSubmitDetail({
                                                                DOCNO: docno,
                                                                SEQ: seq,
                                                                DATA_LIST: []
                                                            });
                                                        }
                                                    });
                                                }
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
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        }
                    });
                }
            }
        },
        {
            text: '撥發量同申請量',
            id: 'btnDetailAPPQTY2APVQTY',
            name: 'btnDetailAPPQTY2APVQTY',
            disabled: true,
            handler: function () {
                var selection = T2Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    selection.forEach(function (record) {
                        record.set('APVQTY', record.get('APPQTY'));
                    });
                }
            }

        },
        {
            xtype: 'checkbox',
            boxLabel: '過濾已核撥',
            listeners: {
                change: function (checkbox, newValue, oldValue, eOpts) {
                    if (newValue) {
                        // 如果 Checkbox 被选中，使用 filterBy 方法过滤 Grid 数据
                        T2Store.filterBy(function (record) {
                            // 根据 name 字段过滤掉包含 'D' 的记录
                            return !record.get('POSTID').toLowerCase().includes('d');
                        });
                    } else {
                        // 如果 Checkbox 取消选中，清除过滤条件
                        T2Store.clearFilter();
                    }
                }
            }
        }]
    });


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
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
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
            text: "院內碼",
            dataIndex: 'MMCODE',
            width: 100,
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
            text: "申請量",
            dataIndex: 'APPQTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: "<font color=red>核撥量</font>",
            dataIndex: 'APVQTY',
            style: 'text-align:left',
            width: 80, align: 'right',
            editor: {
                xtype: 'textfield',
                regexText: '只能輸入數字',
                regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            }
        }, {
            text: "上級庫庫房存量",
            dataIndex: 'S_INV_QTY',
            style: 'text-align:left',
            width: 120, align: 'right'
        }, {
            text: '過帳狀態',
            dataIndex: 'POSTID',
            width: 120
        }, {
            text: "優惠最小單價",
            dataIndex: 'DISC_UPRICE',
            style: 'text-align:left',
            width: 120, align: 'right'
        }, {
            text: "欠撥原因",
            dataIndex: 'SHORT_REASON',
            width: 100
        }, {
            text: "備註",
            dataIndex: 'APLYITEM_NOTE',
            width: 170,
            maxLength: 50
        }, {
            text: "病患姓名",
            dataIndex: 'CHINNAME',
            width: 80
        }, {
            text: '核可時間',
            dataIndex: 'DIS_TIME',
            width: 120
        }, {
            text: '核可人員',
            dataIndex: 'DIS_USER',
            width: 120
        }, {
            text: '撥發人員',
            dataIndex: 'APVID',
            width: 120
        }, {
            text: '撥發時間',
            dataIndex: 'APVTIME',
            width: 120
        }, {
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                        // 編輯前紀錄要用到的值(使用者採用點其他列的資料完成編輯會導致T1LastRec被更新)
                        if ((T1F3 == '2') || (T1F3 == '02') || (T1F3 == '4') || (T1F3 == '03')) {
                            return true;
                        }
                        else {
                            return false; // 取消row editing模式
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                        if (context.colIdx == 6 && context.value != "") {
                            if (T2LastRec != null) {
                                MaxValue = T2LastRec.data['APPQTY'];
                                sInvQty = T2LastRec.data['S_INV_QTY'];
                            }
                            if (Number(context.value) <= 0) {
                                Ext.MessageBox.alert('錯誤', '核撥量不得小於等於0');
                                context.cancel = true;
                                context.record.data[context.field] = context.originalValue;
                            } else {
                                if (Number(context.value) > Number(MaxValue)) {
                                    Ext.MessageBox.alert('錯誤', '核撥量不得超過申請量');
                                    context.cancel = true;
                                    context.record.data[context.field] = context.originalValue;
                                } else if (Number(context.value) > Number(sInvQty)) {
                                    Ext.MessageBox.alert('錯誤', '核撥量不得超過上級庫庫房存量');
                                    context.cancel = true;
                                    context.record.data[context.field] = context.originalValue;
                                }
                            }
                        }
                    }
                }
            })
        ],
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
            }
        }
    });

    function setSubmitDetail(args) {
        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
        myMask.show();
        Ext.Ajax.request({
            url: '/api/AA0147/UpdatePostidBySP',
            method: reqVal_p,
            params: {
                DOCNO: args.DOCNO,
                SEQ: args.SEQ,
                DATA_LIST: Ext.util.JSON.encode(args.DATA_LIST)
            },
            //async: true,
            success: function (response) {
                myMask.hide();
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                    T1Load();
                    msglabel('訊息區:單筆撥發成功');
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
    /*function chkExpDetail(parDocno, parSeq) {
        Ext.Ajax.request({
            url: '/api/AA0147/ChkExpDetail',
            params: {
                docno: parDocno,
                seq: parSeq
            },
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == 'Y') {
                        Ext.MessageBox.confirm('單筆撥發', '是否確定單筆撥發?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0147/UpdatePostidBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: parDocno,
                                        SEQ: parSeq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T1Load();
                                            T1LastRec = null;
                                            T2Load();
                                            Ext.getCmp('detailSubmit').setDisabled(true);
                                            msglabel('單筆撥發成功');
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
                        Ext.Msg.alert('訊息', '項次' + data.msg + '效期管制資料尚未維護完成。');
                    }
                }
            }
        });
    }
    */
    function PrintReport() {

        var selection = T1Grid.getSelection();
        var docnos = '';
        /*for (var i = 0; i < selection.length; i++) {
            if (docnos != '') {
                docnos += ',';
            }
            docnos += selection[i].data.DOCNO;
        }*/
        docnos = T1LastRec.data.DOCNO;
        if (!win) {
            var np = {
                p5: T1F1,
                Action: 1
            };
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?docno=' + docnos + '&Order=mmcode" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0" style="background-color:#FFFFFF"></iframe>',
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

    //view 
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

    function right(str, num) {
        return str.substring(str.length - num, str.length)
    }
    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('P0').focus();
});
