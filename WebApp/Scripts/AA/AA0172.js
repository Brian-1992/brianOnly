Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);
Ext.onReady(function () {
    var T1Set = ''; // 新增/修改/刪除
    var T1Name = "核可確認";
    var T1Rec = 0;
    var T1LastRec = null;
    var T1Name = "";
    var T2Name = "";
    var T1F1 = '';
    var arrP3 = [];
    var hosp_code = '';

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

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        listeners: {
            load: function (store, eOpts) {
                hosp_code = store.getAt(0).get('HOSP_CODE');
            }
        },
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0172/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });


    var st_Flowid = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT', 'COMBITEM', 'EXTRA1']
    });

    var st_apply_kind = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0172/GetApplyKindCombo',
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
            url: '/api/AA0172/GetAppDeptCombo',
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
            url: '/api/AA0172/GetMatclassCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var T1QuerymmCodeCombo = Ext.create('WEBAPP.form.MMCodeCombo', {
        id: 'P7',
        name: 'P7',
        fieldLabel: '院內碼',
        emptyText: '全部',
        width: 200,
        limit: 10,
        queryUrl: '/api/AA0172/GetMMCodeCombo',
        extraFields: ['MMCODE', 'MMNAME_C', 'MMNAME_E'],
        getDefaultParams: function () {

        },
        listeners: {
        }
    });
    function setFlowidComboData() {
        Ext.Ajax.request({
            url: '/api/AA0172/GetFlowidCombo',
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
            T1QuerymmCodeCombo
            , {
                xtype: 'textfield',
                fieldLabel: '單據號碼',
                name: 'P8',
                id: 'P8',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>'
                //}, {
                //    xtype: 'checkboxfield',
                //    fieldLabel: '進貨待補轉單',
                //    name: 'P6',
                //    id: 'P6',
                //    inputValue: '1',
                //    padding: '0 4 0 4'
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
            { name: 'SRCDOCYN', type: 'string' },
            { name: 'ISARMY_N', type: 'string' },
            { name: 'APPUNA', type: 'string' },
            { name: 'POSTID', type: 'string' }
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 100, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'DESC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AA0172/AllM',
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
                    //p6: T1QueryForm.getForm().findField('P6').rawValue,  //進貨待補轉單
                    p7: T1QueryForm.getForm().findField('P7').rawValue,
                    p8: T1QueryForm.getForm().findField('P8').rawValue
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

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'applyx', text: '退回', disabled: true, handler: function () {
                    var tempData = T2Grid.getStore().data.items;
                    var data = [];
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
                            $.map(selection, function (item, key) {
                                name += '「' + item.get('DOCNO') + '」<br>';
                                docno += item.get('DOCNO') + ',';
                            })
                            Ext.MessageBox.confirm('退回', '是否確定退回？單號如下<br>' + name, function (btn, text) {
                                if (btn === 'yes') {
                                    Ext.Ajax.request({
                                        url: '/api/AA0172/ApplyX',
                                        method: reqVal_p,
                                        params: {
                                            DOCNO: docno
                                        },
                                        //async: true,
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                //Ext.MessageBox.alert('訊息', '核撥成功');
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
                            });
                        }
                    }
                }
            }, {
                //1121021 核可隱藏改用單筆核可
                itemId: 'apply', text: '核可', disabled: true, hidden: true, handler: function () {
                    var tempData = T2Grid.getStore().data.items;
                    var data = [];
                    var isdirty = false;
                    var vMsg = '';
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
                            $.map(selection, function (item, key) {
                                name += '「' + item.get('DOCNO') + '」<br>';
                                docno += item.get('DOCNO') + ',';
                            })

                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            Ext.Ajax.request({
                                url: '/api/AA0172/ChkIsTransPr',
                                params: {
                                    docno: docno
                                },
                                method: reqVal_p,
                                success: function (response) {
                                    myMask.hide();
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        var extraMsg = '';
                                        var msgBtns;
                                        if (data.msg != 'Y') {
                                            extraMsg = '註記轉申購單品項是否轉訂單?';
                                            msgBtns = Ext.MessageBox.YESNOCANCEL;
                                        } else {
                                            extraMsg = '請確認是否核可？';
                                            msgBtns = Ext.MessageBox.OKCANCEL;
                                        }
                                        Ext.Msg.show({
                                            title: '核可',
                                            //    width: '90%',
                                            message: extraMsg,
                                            buttons: msgBtns,
                                            multiLine: true,
                                            fn: function (btn) {
                                                if (btn === 'yes') {
                                                    myMask.show();
                                                    Ext.Ajax.request({
                                                        url: '/api/AA0172/Apply',
                                                        method: reqVal_p,
                                                        params: {
                                                            DOCNO: docno,
                                                            autoTransPr: 'Y'
                                                        },
                                                        //async: true,
                                                        success: function (response) {
                                                            myMask.hide();
                                                            var data = Ext.decode(response.responseText);
                                                            if (data.success) {
                                                                T2Store.removeAll();
                                                                T1Grid.getSelectionModel().deselectAll();
                                                                T1Load();
                                                                msglabel('訊息區:核可成功');
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
                                                else if ((btn == 'no') || (btn == 'ok')) //系統不自動或不需產生訂單及新單據
                                                {
                                                    myMask.show();
                                                    Ext.Ajax.request({
                                                        url: '/api/AA0172/Apply',
                                                        method: reqVal_p,
                                                        params: {
                                                            DOCNO: docno,
                                                            autoTransPr: 'N'
                                                        },
                                                        //async: true,
                                                        success: function (response) {
                                                            myMask.hide();
                                                            var data = Ext.decode(response.responseText);
                                                            if (data.success) {
                                                                T2Store.removeAll();
                                                                T1Grid.getSelectionModel().deselectAll();
                                                                T1Load();
                                                                msglabel('訊息區:核可成功');
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
                                            }
                                        });
                                    }
                                }
                            });
                        }
                    }
                }
            }, {
                //1121021 配合國軍需求將核可鈕隱藏改用單筆核可鈕
                itemId: 'applyc', text: '取消核可', disabled: true, hidden: true, handler: function () {
                    var tempData = T2Grid.getStore().data.items;
                    var data = [];
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
                            $.map(selection, function (item, key) {
                                name += '「' + item.get('DOCNO') + '」<br>';
                                docno += item.get('DOCNO') + ',';
                            })
                            Ext.MessageBox.confirm('取消核可', '是否確定取消核可？單號如下<br>' + name, function (btn, text) {
                                if (btn === 'yes') {
                                    Ext.Ajax.request({
                                        url: '/api/AA0172/ApplyC',
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
                                                msglabel('訊息區:取消核可成功');
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
                }
            }, {
                itemId: 'export', text: '匯出', disabled: true,
                handler: function () {
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

                        Ext.MessageBox.confirm('匯出', '是否確定匯出？單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                var p = new Array();
                                p.push({ name: 'FN', value: '請領單.xls' });
                                p.push({ name: 'DOCNO', value: docno });
                                PostForm('/api/AA0172/Excel', p);
                                msglabel('匯出完成');
                            }
                        });
                    }
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
            text: "申請日期",
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
            width: 200
        }, {
            text: "軍民別",
            dataIndex: 'ISARMY_N',
            width: 100
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
                var tempData = T2Grid.getStore().data.items;
                var data = [];
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

    function setFormT1a() {
        T1Grid.down('#apply').setDisabled(T1Rec === 0);
        T1Grid.down('#applyx').setDisabled(T1Rec === 0);
        T1Grid.down('#applyc').setDisabled(T1Rec === 0);   //取消核可
        T1Grid.down('#export').setDisabled(T1Rec === 0);
        Ext.getCmp('btnUpdate2').setDisabled(T1Rec === 0);   //更新
        Ext.getCmp('btnCancel2').setDisabled(T1Rec === 0);    //取消
        Ext.getCmp('btnDetailApply').setDisabled(T1Rec === 0);  //單筆核可
        Ext.getCmp('btnDetailAPPQTY2PR_QTY').setDisabled(T1Rec === 0); //申購量同申請量
        Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(T1Rec === 0); //核撥量同申請量
        Ext.getCmp('btnDetailCancel').setDisabled(T1Rec === 0);  //單筆取消核可
        Ext.getCmp('btnUpdate22').setDisabled(T1Rec === 0);
        Ext.getCmp('btnCancel22').setDisabled(T1Rec === 0);
        if (T1LastRec) {
            T1F1 = T1LastRec.data["DOCNO"];
            if ((T1LastRec.data["DOCTYPE"] == "MR") || (T1LastRec.data["DOCTYPE"] == "MS")) {
                T1F3 = right(T1LastRec.data["FLOWID"], 2);
            } else {
                T1F3 = T1LastRec.data["FLOWID"];
            }
            if (T1F3 === '11') {
                T1Grid.down('#apply').setDisabled(false);   //核可
                T1Grid.down('#applyx').setDisabled(false);  //退回
                T1Grid.down('#applyc').setDisabled(true);   //取消核可
                Ext.getCmp('btnUpdate2').setDisabled(false);   //更新
                Ext.getCmp('btnCancel2').setDisabled(false);    //取消
                Ext.getCmp('btnDetailApply').setDisabled(false);  //單筆核可
                Ext.getCmp('btnDetailAPPQTY2PR_QTY').setDisabled(false);//申購量同申請量
                Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(false);//核撥量同申請量
                Ext.getCmp('btnDetailCancel').setDisabled(false);  //單筆取消核可
                Ext.getCmp('btnUpdate22').setDisabled(false);
                Ext.getCmp('btnCancel22').setDisabled(false);
            }
            else {
                if ((T1F3 == '2') || (T1F3 == '02')) {
                    T1Grid.down('#applyc').setDisabled(false);
                    if (T1LastRec.data["POSTID"] > 0) {         //POSTID還有NULL或A
                        T1Grid.down('#apply').setDisabled(false);
                        Ext.getCmp('btnDetailApply').setDisabled(false);
                        Ext.getCmp('btnDetailAPPQTY2PR_QTY').setDisabled(false);
                        Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(false);
                        Ext.getCmp('btnDetailCancel').setDisabled(false);
                        Ext.getCmp('btnUpdate2').setDisabled(false);
                        Ext.getCmp('btnCancel2').setDisabled(false);
                    } else {
                        T1Grid.down('#apply').setDisabled(true);
                        Ext.getCmp('btnDetailApply').setDisabled(true);
                        Ext.getCmp('btnDetailAPPQTY2PR_QTY').setDisabled(true);
                        Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(true);
                        Ext.getCmp('btnDetailCancel').setDisabled(true);
                        Ext.getCmp('btnUpdate2').setDisabled(true);
                        Ext.getCmp('btnCancel2').setDisabled(true);
                    }
                } else {
                    T1Grid.down('#applyc').setDisabled(true);
                    Ext.getCmp('btnDetailApply').setDisabled(true);
                    Ext.getCmp('btnDetailAPPQTY2PR_QTY').setDisabled(true);
                    Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(true);
                    Ext.getCmp('btnDetailCancel').setDisabled(true);
                    Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnCancel2').setDisabled(true);
                }
                T1Grid.down('#applyx').setDisabled(true);
                Ext.getCmp('btnDetailAPPQTY2PR_QTY').setDisabled(true);
                Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(true);
                Ext.getCmp('btnUpdate22').setDisabled(true);
                Ext.getCmp('btnCancel22').setDisabled(true);
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
    var MaxValue = 0;
    var InvQty = 0;

    var T2QueryMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P1',
        fieldLabel: '院內碼',
        labelAlign: 'right',
        limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AA0172/GetMMCodeDocd', //指定查詢的Controller路徑
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
            { name: 'DISC_UPRICE', type: 'string' },
            { name: 'ISTRANSPR', type: 'string' },
            { name: 'SHORT_REASON', type: 'string' },
            { name: 'M_CONTID', type: 'string' },
            { name: 'CASENO', type: 'string' },
            { name: 'E_CODATE', type: 'string' },
            { name: 'CHINNAME', type: 'string' },
            { name: 'CHARTNO', type: 'string' },
            { name: 'POSTID', type: 'string' },
            { name: 'DIS_USER', type: 'string' },
            { name: 'DIS_TIME', type: 'string' },
            { name: 'REST_QTY', type: 'string' }
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
            url: '/api/AA0172/AllD',
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

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [{
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
                    url: '/api/AA0172/UpdateMeDocd',
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
            text: '單筆核可',
            id: 'btnDetailApply',
            name: 'btnDetailApply',
            disabled: true,
            handler: function () {
                var tempData = T2Grid.getStore().data.items;
                var data = [];
                var isdirty = false;
                var vMsg = '';
                for (var i = 0; i < tempData.length; i++) {
                    if (tempData[i].dirty) {
                        isdirty = true;
                    }
                }
                if (isdirty) {
                    Ext.Msg.alert('提醒', '尚有資料未儲存');
                }
                else {
                    var selection = T2Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let docno = "", seq = "";
                        let istrans = "N";
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                            if (parseInt(item.get('PR_QTY')) > 0) { //若有筆申購量>0
                                istrans = "Y";
                            }
                        })
                        var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                        myMask.show();
                        if (istrans == 'Y') {
                            extraMsg = '輸入轉申購量之品項是否轉訂單 ? ';
                            msgBtns = Ext.MessageBox.YESNOCANCEL;
                        } else {
                            extraMsg = '請確認是否單筆核可？';
                            msgBtns = Ext.MessageBox.OKCANCEL;
                        }
                        Ext.Msg.show({
                            title: '單筆核可',
                            //    width: '90%',
                            message: extraMsg,
                            buttons: msgBtns,
                            multiLine: true,
                            fn: function (btn) {
                                if (btn == 'yes') {
                                    istrans = 'Y';
                                }
                                else if ((btn == 'no') || (btn == 'ok')) { //系統不自動或不需產生訂單及新單據
                                    istrans = 'N';
                                }

                                if (btn != 'cancel') {    //取消跳過
                                    Ext.Ajax.request({
                                        url: '/api/AA0172/DetailApply', //UpdatePostid
                                        method: reqVal_p,
                                        params: {
                                            DOCNO: docno,
                                            SEQ: seq,
                                            autoTransPr: istrans
                                        },
                                        success: function (response) {
                                            var data = Ext.decode(response.responseText);
                                            if (data.success) {
                                                T1Load();
                                                T1LastRec = null;
                                                T2Load();
                                                Ext.getCmp('btnDetailApply').setDisabled(true);
                                                Ext.getCmp('btnDetailAPPQTY2PR_QTY').setDisabled(true);
                                                Ext.getCmp('btnDetailAPPQTY2APVQTY').setDisabled(true);
                                                msglabel('訊息區:單筆核可成功');
                                            }
                                            else
                                                Ext.MessageBox.alert('錯誤', data.msg);
                                        },
                                        failure: function (response) {
                                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                        }
                                    });
                                }
                                myMask.hide();
                            }
                        });
                    }
                }
            }
        },
        {
            text: '申購量同申請量',
            id: 'btnDetailAPPQTY2PR_QTY',
            name: 'btnDetailAPPQTY2PR_QTY',
            disabled: true,
            handler: function () {
                var selection = T2Grid.getSelection();
                if (selection.length === 0) {
                    Ext.Msg.alert('提醒', '請勾選項目');
                }
                else {
                    selection.forEach(function (record) {
                        record.set('PR_QTY', record.get('APPQTY'));
                        record.set('APVQTY', 0);
                    });
                }
            }

        }, {
            text: '核撥量同申請量',
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
                        record.set('PR_QTY', 0);
                    });
                }
            }
        },
        {
            text: '單筆取消核可',
            id: 'btnDetailCancel',
            name: 'btnDetailCancel',
            disabled: true,
            handler: function () {
                var tempData = T2Grid.getStore().data.items;
                var data = [];
                var isdirty = false;
                var vMsg = '';
                for (var i = 0; i < tempData.length; i++) {
                    if (tempData[i].dirty) {
                        isdirty = true;
                    }
                }
                if (isdirty) {
                    Ext.Msg.alert('提醒', '尚有資料未儲存');
                }
                else {
                    var selection = T2Grid.getSelection();
                    if (selection.length === 0) {
                        Ext.Msg.alert('提醒', '請勾選項目');
                    }
                    else {
                        let seq = '';
                        let docno = '';
                        $.map(selection, function (item, key) {
                            seq += item.get('SEQ') + ',';
                            docno += item.get('DOCNO') + ',';
                        })
                        Ext.MessageBox.confirm('單筆取消核可', '是否確定單筆取消核可?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AA0172/UpdatePostidNull',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            T1Load();
                                            T1LastRec = null;
                                            T2Load();
                                            Ext.getCmp('btnDetailCancel').setDisabled(true);
                                            msglabel('訊息區:單筆取消核可成功');
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
            }
        }, {
            text: '設定轉申購',
            id: 'btnUpdate22',
            name: 'btnUpdate22',
            disabled: true,
            hidden: true,  //國軍無此需求故先隱藏(因flowid=未核可由系統自動判斷，人工設定會有問題)
            handler: function () {
                var selection = T2Grid.getSelection();
                if (selection.length) {
                    let seq = '';
                    let docno = '';
                    $.map(selection, function (item, key) {
                        seq += item.get('SEQ') + ',';
                        docno += item.get('DOCNO') + ',';
                    })

                    Ext.MessageBox.confirm('設定轉申購', '確認是否送出?', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0172/UpdateMeDocdPrY',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    SEQ: seq
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T2Load();
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
                    Ext.MessageBox.alert('錯誤', '尚未選取院內碼');
                    return;
                }
            }
        }, {
            text: '取消設定轉申購',
            id: 'btnCancel22',
            name: 'btnCancel22',
            disabled: true,
            hidden: true,  //國軍無此需求故先隱藏(因flowid=未核可由系統自動判斷，人工設定會有問題)
            handler: function () {
                var selection = T2Grid.getSelection();
                if (selection.length) {
                    let seq = '';
                    let docno = '';
                    $.map(selection, function (item, key) {
                        seq += item.get('SEQ') + ',';
                        docno += item.get('DOCNO') + ',';
                    })

                    Ext.MessageBox.confirm('取消設定轉申購', '確認是否送出?', function (btn, text) {
                        if (btn === 'yes') {
                            Ext.Ajax.request({
                                url: '/api/AA0172/UpdateMeDocdPrN',
                                method: reqVal_p,
                                params: {
                                    DOCNO: docno,
                                    SEQ: seq
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        T2Load();
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
                    Ext.MessageBox.alert('錯誤', '尚未選取院內碼');
                    return;
                }
            }
        }
        ]
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
            text: "<font color=red>申購量</font>",
            dataIndex: 'PR_QTY',
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
            text: "結存量",
            dataIndex: 'REST_QTY',
            style: 'text-align:left',
            width: 80, align: 'right'
        }, {
            text: '過帳狀態',
            dataIndex: 'POSTID',
            width: 120
        }, {
            text: "合約識別碼",
            dataIndex: 'M_CONTID',
            width: 90
        }, {
            text: "合約案號",
            dataIndex: 'CASENO',
            width: 100
        }, {
            text: "合約效期",
            dataIndex: 'E_CODATE',
            width: 80
        }, {
            text: "優惠最小單價",
            dataIndex: 'DISC_UPRICE',
            style: 'text-align:left',
            width: 120, align: 'right'
            //},{
            //    text: "轉申購",
            //    dataIndex: 'ISTRANSPR',
            //    width: 60
        }, {
            text: "欠撥原因",
            dataIndex: 'SHORT_REASON',
            width: 100
        }, {
            text: "<font color=red>備註</font>",
            dataIndex: 'APLYITEM_NOTE',
            width: 170,
            maxLength: 50,
            editor: {
                xtype: 'textfield'
            }
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
            header: "",
            flex: 1
        }],
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,//控制點擊幾下啟動編輯
                listeners: {
                    beforeedit: function (context, eOpts) {
                        // 編輯前紀錄要用到的值(使用者採用點其他列的資料完成編輯會導致T1LastRec被更新)
                        debugger
                        if ((T1F3 === '11') || (T1F3 == "2")) {
                            //POSTID=未核定才可填寫
                            var editColumnIndex1 = findColumnIndex(T2Grid.columns, 'APVQTY');
                            var editColumnIndex2 = findColumnIndex(T2Grid.columns, 'PR_QTY');
                            var editColumnIndex3 = findColumnIndex(T2Grid.columns, 'APLYITEM_NOTE');
                            if (((eOpts.colIdx - 1 == editColumnIndex1) || (eOpts.colIdx - 1 == editColumnIndex2) || (eOpts.colIdx - 1 == editColumnIndex3))&& eOpts.record.get('POSTID') == '待核可'){
                                return true;
                            } else {
                                return false;
                            }
                        }
                        else {
                            return false; // 取消row editing模式
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                        if (context.colIdx == 6 && context.value != "") {
                            if (T2LastRec != null) {
                                MaxValue = T2LastRec.data['APPQTY'];
                                InvQty = T2LastRec.data['INV_QTY'];
                            }
                            if (Number(context.value) < 0) {
                                Ext.MessageBox.alert('錯誤', '核撥量不得小於0');
                                context.cancel = true;
                                context.record.data[context.field] = context.originalValue;
                            } else {
                                if (Number(context.value) > Number(MaxValue)) {
                                    Ext.MessageBox.alert('錯誤', '核撥量不得超過申請量');
                                    context.cancel = true;
                                    context.record.data[context.field] = context.originalValue;
                                }
                            }
                        }
                        else if (context.colIdx == 7 && context.value != "") {
                            if (T2LastRec != null) {
                                MaxValue = T2LastRec.data['APPQTY'];
                                PrQty = T2LastRec.data['PR_QTY'];
                            }
                            if (Number(context.value) < 0) {
                                Ext.MessageBox.alert('錯誤', '申購量不得小於0');
                                context.cancel = true;
                                context.record.data[context.field] = context.originalValue;
                            } else {
                                //807 除了松山以外 要檢查
                                if (Number(context.value) > Number(MaxValue) && hosp_code != '807') {
                                    Ext.MessageBox.alert('錯誤', '申購量不得超過申請量');
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
                //viewport.down('#form').addCls('T1b');
            }
        }
    });

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
                items: [{
                    region: 'north',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    split: true,
                    height: '30%',
                    items: [T1Grid]
                }, {
                    region: 'center',
                    layout: 'fit',
                    collapsible: false,
                    title: '',
                    height: '70%',
                    split: true,
                    items: [T2Grid]
                }]
            }]
        }, {
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
        }]
    });

    function right(str, num) {
        return str.substring(str.length - num, str.length)
    }

    var findColumnIndex = function (columns, dataIndex) {
        var index;
        for (index = 0; index < columns.length; ++index) {
            if (columns[index].dataIndex == dataIndex) { break; }
        }
        return index == columns.length ? -1 : index;
    }
    //T1Load(); // 進入畫面時自動載入一次資料
    T1QueryForm.getForm().findField('P0').focus();

});
