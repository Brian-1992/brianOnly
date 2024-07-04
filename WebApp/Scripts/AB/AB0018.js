Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    var T1GetExcel = '/../../../api/AB0018/GetExcel';   //查詢點收差異資料匯出

    //var T1Name = "藥局進貨作業";
    var T1Rec = 0;
    var T1LastRec = null;
    //var IsPageLoad = true;
    // var pSize = 100; //分頁時每頁筆數


    var col1_labelWid = 130;
    var col1_Wid = 280;
    var col2_labelWid = 130;
    var col2_Wid = 260;
    var col3_labelWid = 110;
    var col3_Wid = 300;
    var f2_wid = (col1_Wid + col2_Wid + col3_Wid) / 6;
    var f3_wid = (col1_Wid + col2_Wid + col3_Wid) / 4;
    var f4_wid = (col1_Wid + col2_Wid + col3_Wid) / 5;
    var mLabelWidth = 70;
    var mWidth = 180;
    var Dno;
    var T1F3 = "";
    var T2F3 = "";

    var user_task = '';
    var user_tflow = '';
    var T2Update = '/api/AB0018/UpdateMedocD';
    var StatusQueryStore = Ext.create('Ext.data.Store', {
        fields: ['KEY_CODE', 'COMBITEM']
    });

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0018/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    function getUsertask() {
        Ext.Ajax.request({
            url: '/api/AB0018/GetUsertask',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                user_task = data;
            },
            failure: function (response, options) {

            }
        });
    }
    getUsertask();

    function getUsertflow() {
        Ext.Ajax.request({
            url: '/api/AB0018/GetUsertflow',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                user_tflow = data;
            },
            failure: function (response, options) {

            }
        });
    }
    getUsertflow();
    function setComboData() {
        Ext.Ajax.request({
            url: '/api/AB0018/GetStatusCombo',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var tb_vendor = data.etts;
                    if (tb_vendor.length > 0) {
                        for (var i = 0; i < tb_vendor.length; i++) {
                            StatusQueryStore.add({ KEY_CODE: tb_vendor[i].KEY_CODE, COMBITEM: tb_vendor[i].COMBITEM });
                        }
                    }
                }
            },
            failure: function (response, options) {

            }
        });
    }
    setComboData();
    //var cb = Ext.create('WEBAPP.form.ParamCombo', {
    //    name: 'P8',
    //    id: 'P8',
    //    fieldLabel: '申請狀態',
    //    queryParam: {
    //        GRP_CODE: 'ME_DOCM',
    //        DATA_NAME: 'FLOWID'
    //    },
    //    width: 170,
    //    labelWidth: 60,
    //    insertEmptyRow: true,
    //    autoSelect: true,
    //    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true
    //});

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'form',
        border: false,
        autoScroll: true,
        fieldDefaults: {
            labelAlign: 'right',
            labelWidth: mLabelWidth,
            width: mWidth
        }
        ,
        items: [{
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '申請單號',
                    name: 'P0',
                    id: 'P0',
                    width: 200,
                    padding: '0 4 0 4',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 90
                }, {
                    xtype: 'textfield',
                    fieldLabel: '核發庫房',
                    name: 'P3',
                    id: 'P3',
                    padding: '0 4 0 4',
                    enforceMaxLength: true,
                    width: 180,
                    maxLength: 90
                }, {

                    xtype: 'textfield',
                    id: 'P1',
                    name: 'P1',
                    fieldLabel: '申請日期',
                    // format: 'Y/m/d',
                    width: 180,
                    enforceMaxLength: true, //最多輸入的長度有限制
                    maxLength: 7,          //最多輸入10碼
                    maskRe: /[0-9]/,      //前端只能輸入數字跟斜線
                    regexText: '正確格式應為「YYY/MM/DD」，<br>例如「1080101」',
                    regex: /^[0~9]{7}$/,
                }, {
                    xtype: 'textfield',
                    id: 'P2',
                    name: 'P2',
                    fieldLabel: '至',
                    labelSeparator: '',
                    width: 130,
                    labelWidth: 20,
                    enforceMaxLength: true, //最多輸入的長度有限制
                    maxLength: 7,          //最多輸入10碼
                    maskRe: /[0-9\/]/,      //前端只能輸入數字跟斜線
                    regexText: '正確格式應為「YYY/MM/DD」，<br>例如「1080101」',
                    regex: /^[0~9]{7}$/,
                    //}, {
                    //    xtype: 'textfield',
                    //    fieldLabel: '使用部門',
                    //    name: 'P6',
                    //    id: 'P6',
                    //    width: 180,
                    //    enforceMaxLength: true,
                    //    maxLength: 90//,
                    //   // padding: '0 4 0 4'
                    //}, {
                    //    xtype: 'textfield',
                    //    fieldLabel: '領用庫房',
                    //    name: 'P7',
                    //    id: 'P7',
                    //    width: 180,
                    //    enforceMaxLength: true,
                    //    maxLength: 90//,
                    // padding: '0 4 0 4'
                    //}, {
                    //    xtype: 'combo',
                    //    store: StatusQueryStore,
                    //    id: 'P8',
                    //    name: 'P8',
                    //    displayField: 'COMBITEM',
                    //    valueField: 'KEY_CODE',
                    //    fieldLabel: '申請狀態',
                    //    queryMode: 'local',
                    //    matchFieldWidth: false,
                    //    listConfig: {
                    //        width: 350
                    //    },
                    //    autoSelect: true,
                    //    //multiSelect: true,
                    //    editable: true, typeAhead: true, forceSelection: true, selectOnFocus: true, enableKeyEvents: true


                    //}, {
                    //    xtype: 'button',
                    //    text: '查詢',
                    //    handler: T1Load
                    //}, {
                    //    xtype: 'button',
                    //    text: '清除',
                    //    handler: function () {
                    //        var f = this.up('form').getForm();
                    //        f.reset();
                    //        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                    //    }
                }
            ]
        }, {
            xtype: 'panel',
            id: 'PanelP2',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '使用部門',
                    name: 'P6',
                    id: 'P6',
                    width: 200,
                    enforceMaxLength: true,
                    maxLength: 9,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '領用庫房',
                    name: 'P7',
                    id: 'P7',
                    width: 180,
                    enforceMaxLength: true,
                    maxLength: 9,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'label',
                    width: 210
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: T1Load
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
        }]
    });

    var st_towhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0018/GetTowhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true,
        listeners: {
            load: function (store, records, successful, eOpts) {
                if (records.length > 0) {
                    T1QueryForm.getForm().findField('P7').setValue(records[0].data.VALUE);
                }
            }
        }
    });
    var st_frwhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0018/GetFrwhCombo',
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
            url: '/api/AB0018/GetFlowidCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
    //var tflow = st_getlogininfo.getAt(0).get('TASK_ID');
    
    var T1QueryForm = Ext.widget({
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
                xtype: 'textfield',
                fieldLabel: '申請單號',
                name: 'P0',
                id: 'P0',
                width: 200,
                padding: '0 4 0 4',
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 90
            }, {
                xtype: 'combo',
                fieldLabel: '核撥庫房',
                name: 'P3',
                id: 'P3',
                store: st_frwhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {

                xtype: 'datefield',
                id: 'P1',
                name: 'P1',
                fieldLabel: '申請日期',
                width: 180,
                vtype: 'dateRange',
                dateRange: { end: 'P2' },
                value: getToday()
            }, {
                xtype: 'datefield',
                id: 'P2',
                name: 'P2',
                fieldLabel: '至',
                labelSeparator: '',
                width: 130,
                labelWidth: 20,
                vtype: 'dateRange',
                dateRange: { begin: 'P1' }
            }, {
                xtype: 'combo',
                fieldLabel: '領用庫房',
                name: 'P7',
                id: 'P7',
                store: st_towhcombo,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, {
                xtype: 'combo',
                fieldLabel: '單據狀態',
                name: 'P8',
                id: 'P8',
                store: st_Flowid,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }
        ],
        buttons: [{
            itemId: 'query', text: '查詢',
            handler: T1Load
        }, {
            itemId: 'clean', text: '清除', handler: function () {
                var f = this.up('form').getForm();
                f.reset();
                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                //T1QueryForm.getForm().findField('P8').setValue(arrP8);
                //T1QueryForm.getForm().findField('P7').setValue(st_towhcombo.getAt(0).get('VALUE'));
            }
        }]
    });

    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: ['DOCNO', 'FLOWID', 'APPID', 'APPDEPT', 'APPTIME', 'USEID', 'USEDEPT', 'FRWH', 'TOWH', 'MR1', 'FLOWID_N','TOWH_NO']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T1Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T1Model',
        pageSize: 10,
        remoteSort: true,
        sorters: [{ property: 'DOCNO', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: T1QueryForm.getForm().findField('P0').getValue(),
                    p1: T1QueryForm.getForm().findField('P1').rawValue,
                    p2: T1QueryForm.getForm().findField('P2').rawValue,
                    p3: T1QueryForm.getForm().findField('P3').getValue(),
                    //p4: T1Query.getForm().findField('P4').getValue(),
                    //p5: T1Query.getForm().findField('P5').getValue(),
                    //p6: T1QueryForm.getForm().findField('P6').getValue(),
                    p7: T1QueryForm.getForm().findField('P7').getValue(),
                    p8: T1QueryForm.getForm().findField('P8').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0018/QueryME',
            reader: {
                type: 'json',
                rootProperty: 'etts',
                totalProperty: 'rc'
            }
        }

    });

    Ext.define('T2Model', {
        extend: 'Ext.data.Model',
        fields: ['DOCNO', 'MMCODE', 'APPQTY', 'APVQTY', 'PICK_QTY', 'APVTIME', 'APVID', 'dueQty', 'SEQ', 'BASE_UNIT', 'WEXP_ID','TOWH']//ASKING_PERSON', 'RESPONDER', 'ASKING_DATE', 'CONTENT1', 'CHG_DATE', 'content', 'RESPONSE', 'RESPONSE_DATE', 'STATUS']//, 'Plant', 'PR_Create_By', 'RequestUnit', 'PR_DocType', 'Buyer', 'Status'],

    });

    var T2Store = Ext.create('Ext.data.Store', {
        // autoLoad:true,
        model: 'T2Model',
        pageSize: 99999,
        remoteSort: true,
        sorters: [{ property: 'POSTID', direction: 'ASC' }],

        listeners: {
            beforeload: function (store, options) {
                var np = {
                    p0: Dno

                };
                Ext.apply(store.proxy.extraParams, np);
            }
        },

        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0018/QueryMEDOCD',
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
                itemId: 'cancelRestart',
                id: 'btncancel',
                name: 'btncancel',
                text: '結案',
                disabled: true,
                handler: function () {
                    T1Grid.down('#cancelRestart').setDisabled(true);
                    var records = T1Grid.getSelectionModel().getSelection();
                    console.log(records)

                    if (records.length == 0) {
                        Ext.Msg.alert('訊息', '請先選擇結案資料!');
                        T1Grid.down('#cancelRestart').setDisabled(false);
                        return;
                    }

                    Ext.MessageBox.confirm('訊息', '是否確定結案?', function (btn, text) {
                        if (btn === 'yes')
                        {
                            for (var i = 0; i < records.length; i++) {

                                Ext.Ajax.request({
                                    actionMethods: {
                                        read: 'POST' // by default GET
                                    },
                                    url: '/api/AB0018/UpdateEnd',
                                    params: {
                                        dno: records[i].data.DOCNO,
                                        towh_no: records[i].data.TOWH_NO,
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            Ext.Msg.alert('訊息', '結案完成!');
                                            T1LastRec = null;
                                            T2Store.removeAll();
                                            T1Store.load();
                                        }
                                        else
                                        {
                                            Ext.Msg.alert('訊息', data.msg);
                                        }
                                    },
                                    failure: function (response, options) {

                                    }
                                });
                            }
                        }
                        else
                            T1Grid.down('#cancelRestart').setDisabled(false);
                    });
                }
            }, {
                itemId: 'exp', text: '點收效期', disabled: true, handler: function () {
                    var records = T1Grid.getSelectionModel().getSelection();

                    showExpWindow(T1LastRec.data['DOCNO'], 'I', viewport);
                }
            },{
                itemId: 'diff', id: 'diff', text: '查詢點收差異', handler: function () {
                    showWin();
                }
            }],
    });


    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        displayMsg: '顯示{0} - {1}筆,共{2}筆 　　<font color="red">*院內碼紅字為效期管理</font>',
        buttons: [
            //{
            //    itemId: 'bt1', text: '儲存', disabled: false, handler: function () {
            //        var store1 = T2Grid.getStore();
            //        var store = store1.getUpdatedRecords();
            //        //var store = T2Grid.getStore().getRange();
            //        var recs = store;


            //        Ext.MessageBox.confirm('提示', '是否確定儲存?', function (btn, text) {
            //            if (btn === 'yes') {
            //                console.log(recs);
            //                for (var i = 0; i < recs.length; i++) {

            //                    if (recs[i].data.ACKQTY == null) {

            //                        return;
            //                    }



            //                    if (recs[i].data.ACKQTY > recs[i].data.PICK_QTY) {
            //                        Ext.Msg.alert('提示', '點收數量不可大於揀料量');
            //                        //alert('幣別為必填欄位')
            //                        return;
            //                    }

            //                    if (recs[i].data.POSTID != "C") {
            //                        return;
            //                    }
            //                    //recs[i].data.ACKQTY = parseInt(recs[i].data.ACKQTY) + parseInt(recs[i].modified.ACKQTY)


            //                    Ext.Ajax.request({
            //                        async: false,
            //                        url: '/api/AB0018/CheckMEDOCC',
            //                        actionMethods: {
            //                            read: 'POST' // by default GET
            //                        },
            //                        params: {
            //                            p0: recs[i].data.DOCNO,
            //                            p1: recs[i].data.SEQ,
            //                            //p2
            //                        },

            //                        success: function (response) {
            //                            var responseText = Ext.decode(response.responseText),
            //                                newCK = responseText;

            //                            if (newCK.DOCNO == 0) {
            //                                Ext.Ajax.request({

            //                                    url: '/api/AB0018/Update',
            //                                    method: reqVal_p,
            //                                    params: recs[i].data,
            //                                    success: function () {
            //                                        Ext.Msg.alert('提示', '儲存完成');
            //                                        //alert('報價完成');
            //                                        T2Load();
            //                                    },
            //                                    failure: function () {
            //                                        alert('falil');
            //                                    }
            //                                });
            //                            }
            //                            else {
            //                                Ext.Ajax.request({
            //                                    async: false,
            //                                    url: '/api/AB0018/UpdateC',
            //                                    method: reqVal_p,
            //                                    params: recs[i].data,
            //                                    success: function () {
            //                                        Ext.Msg.alert('提示', '儲存完成');
            //                                        //alert('報價完成');
            //                                        T2Load();
            //                                    },
            //                                    failure: function () {
            //                                        alert('falil');
            //                                    }
            //                                });
            //                            }
            //                        },
            //                        failure: function () {
            //                            Ext.Msg.alert('提示', '儲存失敗');

            //                        }
            //                    });

            //                }
            //            }
            //        });
            //    }
            //},
            {
                itemId: 'exp', text: '效期', hidden: true, handler: function () {
                    var records = T1Grid.getSelectionModel().getSelection();

                    showExpWindow(T1LastRec.data['DOCNO'], 'I', viewport);
                }
            },
            {
                itemId: 'applyd',
                id: 'btnapplyd',
                name: 'btnapplyd',
                text: '單筆點收',
                disabled: true,
                handler: function () {
                    Ext.getCmp('btnapplyd').setDisabled(true);

                    var singleApply = true; // 點一次單筆點收只做一次ApplyD                    
                    var store1 = T2Grid.getStore();
                    var store = store1.getUpdatedRecords();
                    var recsall = store;
                    if (recsall.length > 0)
                    {
                        for (var i = 0; i < recsall.length; i++) {
                            if (recsall[i].data.ACKQTY == null) {
                                return;
                            }
                            if (recsall[i].data.ACKQTY > recsall[i].data.PICK_QTY) {
                                Ext.Msg.alert('提示', '點收數量不可大於揀料量');
                                //alert('幣別為必填欄位')
                                return;
                            }
                            if (recsall[i].data.POSTID != "C") {
                                return;
                            }
                            Ext.Ajax.request({
                                async: false,
                                url: '/api/AB0018/CheckMEDOCC',
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                params: {
                                    p0: recsall[i].data.DOCNO,
                                    p1: recsall[i].data.SEQ,
                                    //p2
                                },

                                success: function (response) {
                                    var responseText = Ext.decode(response.responseText),
                                        newCK = responseText;

                                    if (newCK.DOCNO == 0) { //ME_DOCC.isempty
                                        Ext.Ajax.request({

                                            url: '/api/AB0018/Update', //updaet ME_DOCD,ME_DOCC insert ME_DOCC
                                            method: reqVal_p,
                                            params: recsall[i].data,
                                            success: function () {
                                                if (i == (recsall.length - 1) && singleApply)
                                                {
                                                    
                                                    applyDetail(); // check & update的部分做完(做到本次勾選的最後一筆)才做ApplyD的部分
                                                }
                                                    
                                            },
                                            failure: function () {
                                                //alert('falil');
                                            }
                                        });
                                    }
                                    else { //ME_DOCC not empty
                                        Ext.Ajax.request({
                                            async: false,
                                            url: '/api/AB0018/UpdateC',
                                            method: reqVal_p,
                                            params: recsall[i].data,
                                            success: function () {
                                                if (i == (recsall.length - 1) && singleApply)
                                                {
                                                    
                                                    applyDetail();
                                                } 
                                            },
                                            failure: function () {
                                                //alert('falil');
                                            }
                                        });
                                    }
                                },
                                failure: function () {
                                    //Ext.Msg.alert('提示', '儲存失敗');
                                }
                            });

                        }
                    }
                    else
                        
                        applyDetail();
                   
                }
            }
        ]
    });

    var T1Grid = Ext.create('Ext.grid.Panel', {
        menuDisabled: true,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        //plugins: [
        //    Ext.create("Ext.grid.plugin.CellEditing", {
        //        //clicksToEdit: 1,
        //    })
        //],
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T1Tool]
        }],
        columns: [
            { xtype: 'rownumberer' },
            { text: '單據號碼', dataIndex: 'DOCNO', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '單據狀態', dataIndex: 'FLOWID', align: 'left', style: 'text-align:left', menuDisabled: true, width: 120 },
            { text: '申請人員', dataIndex: 'APPID', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },

            //{ text: 'QID', dataIndex: 'QID',align: 'center', style: 'text-align:center', width: 40, sortable: true },
            { text: '申請部門', dataIndex: 'APPDEPT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '申請日期', dataIndex: 'APPTIME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '使用部門', dataIndex: 'USEDEPT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '核發庫房', dataIndex: 'FRWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '領用庫房', dataIndex: 'TOWH', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },



        ],
        selModel: Ext.create('Ext.selection.CheckboxModel'),//check box
        listeners: {
            select: function (grid, record) {
                Dno = record.data.DOCNO;

                T2Load();
            },
            selectionchange: function (model, record) {
                T1LastRec = record[0];
                
                if (record[0]) {
                    var records = T1Grid.getSelectionModel().getSelection();
                    if (records.length == 1)
                    {
                        if (record[0].data.MR1 == "0") {
                            T1Grid.down('#cancelRestart').setDisabled(false);
                            T1Grid.down('#exp').setDisabled(false);
                        }
                        else {
                            T1Grid.down('#cancelRestart').setDisabled(true);
                            T1Grid.down('#exp').setDisabled(true);
                        };
                    }
                    else if (records.length > 1)
                    {
                        var setBtnTF = false;
                        for (var i = 0; i < records.length; i++)
                        {
                            // 多選時有任一筆不可結案則disable結案鈕
                            if (records[i].data.MR1 != "0")
                            {
                                setBtnTF = true;
                            }
                        }  
                        T1Grid.down('#cancelRestart').setDisabled(setBtnTF);
                        T1Grid.down('#exp').setDisabled(setBtnTF);
                    }
                    else
                    {
                        T1Grid.down('#cancelRestart').setDisabled(true);
                        T1Grid.down('#exp').setDisabled(true);
                    }
                    T1F3 = record[0].data.FLOWID_N;

                    T2Grid.down('#applyd').setDisabled(true);
                }
                else
                {
                    T1Grid.down('#cancelRestart').setDisabled(true);
                    T1Grid.down('#exp').setDisabled(true);
                    T2Grid.down('#applyd').setDisabled(true);
                }
            }
        },
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: 'Dno',
        menuDisabled: true,
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            autoScroll: true,
            items: [T2Tool]
        }],
        columns: [
            //{
            //    text: '單據號碼', dataIndex: 'DOCNO', align: 'center', style: 'text-align:center', width: 120,
            //    renderer: function (val, meta, record) {

            //        var param1 = 'AB1801';
            //        var param2 = '領用細項';
            //        var param3 = record.data.DOCNO;
            //        return '<a href=javascript:showPopFormF("' + param1 + '","' + param2 + '","' + param3 + '",Ext.getCmp("viewport"))>' + val + '</a>';
            //    },
            //},
            {
                xtype: 'rownumberer'
            }, {
                text: '院內碼', dataIndex: 'MMCODE', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100,
                renderer: function (val, meta, record) {
                    if (record.data['WEXP_ID'] == 'Y')
                        return '<font color=red>' + val + '</font>';
                    else
                        return val;
                }

            },
            { text: '英文品名', dataIndex: 'MMNAME_E', align: 'left', style: 'text-align:left', menuDisabled: true, width: 200 },
            { text: '單位', dataIndex: 'BASE_UNIT', align: 'left', style: 'text-align:left', menuDisabled: true, width: 70 },
            //{ text: 'QID', dataIndex: 'QID',align: 'center', style: 'text-align:center', width: 40, sortable: true },
            //{
            //    text: "<span style='color: red'>點收數量</span>", dataIndex: 'ACKQTY', align: 'right', style: 'text-align:left', fontColor: 'red', menuDisabled: true, width: 100, editor: {
            //        //xtype: 'numberfield',
            //        //decimalPrecision: 3,
            //        //nValue: 0
            //    }
            //},
            {
                text: "<span style='color:red'>點收數量</span>",
                dataIndex: 'ACKQTY',
                width: 100,
                align: 'right',
                editor: {
                    selectOnFocus: true,
                    fieldStyle: 'text-align:right;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 5);
                            }

                            if (e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 5);
                            }
                        }
                    }
                }
            },
            { text: '申請數量', dataIndex: 'APPQTY', align: 'right', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '核撥數量', dataIndex: 'APVQTY', align: 'right', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '核撥日期', dataIndex: 'APVTIME', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            { text: '核撥人員', dataIndex: 'APVID', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            {
                text: '過帳狀態', dataIndex: 'POSTID', align: 'left', style: 'text-align:left', menuDisabled: true, width: 80,
                renderer: function (val, meta, record) {
                    var rtn = '無';
                    if (val == '3') {
                        rtn = '3 待核撥';
                    }
                    else if (val == '4') {
                        rtn = '4 待點收';
                    }
                    else if (val == 'C') {
                        rtn = 'C 已核撥';
                    }
                    else if (val == 'D') {
                        rtn = 'D 已點收';
                    }
                    return rtn;
                }

            },
            { text: '點收時間', dataIndex: 'ACKTIME',  menuDisabled: true, width: 120 },
            { text: '點收人員', dataIndex: 'ACKID',  menuDisabled: true, width: 120 },
            { text: '強迫點收數量', dataIndex: 'ACKSYSQTY',  menuDisabled: true, width: 120 },
            //{ text: '揀料量', dataIndex: 'PICK_QTY', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            //{ text: '在途數量', dataIndex: 'DUEQTY', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },
            //{ text: '項次', dataIndex: 'SEQ', align: 'left', style: 'text-align:left', menuDisabled: true, width: 100 },

        ]
        ,
        selModel: Ext.create('Ext.selection.CheckboxModel'),//check box
        viewConfig: {
            listeners: {
                selectionchange: function (model, record) {
                    if (record[0]) {
                        var records = T2Grid.getSelectionModel().getSelection();
                        if (records.length == 1)
                        {
                            T2F3 = record[0].data.POSTID;
                            if ((T1F3 == "0102" || T1F3 == "0103" || T1F3 == "0602" || T1F3 == "0603") && T2F3 == 'C') {
                                T2Grid.down('#applyd').setDisabled(false);
                            }
                            else {
                                T2Grid.down('#applyd').setDisabled(true);
                            }
                        }
                        else if (records.length > 1)
                        {
                            var setBtnTF = false;
                            
                            for (var i = 0; i < records.length; i++) {
                                // 多選時有任一筆不可點收則disable單筆點收鈕
                                if (records[i].data.POSTID != "C" ||
                                    (T1F3 != "0102" && T1F3 != "0103" && T1F3 != "0602" && T1F3 != "0603" && T2F3 == 'C')) {
                                    setBtnTF = true;
                                }
                            }  
                            T2Grid.down('#applyd').setDisabled(setBtnTF);
                        }
                        else
                            T2Grid.down('#applyd').setDisabled(true);
                    }
                }
            }
        }
        ,
        plugins: [
            Ext.create("Ext.grid.plugin.CellEditing", {
                clicksToEdit: 1,
                listeners: {
                    beforeedit: function (context, eOpts) {
                        if ((T1F3 == "0102" || T1F3 == "0103" || T1F3 == "0602" || T1F3 == "0603") && T2F3 == "C") {
                            return true;
                        }
                        else {
                            return false; // 取消row editing模式
                        }
                    },
                    validateedit: function (editor, context, eOpts) {
                    },
                    edit: function (editor, context, eOpts) {
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        var docno = r.get('DOCNO');
                        var ackqty = (context.field === 'ACKQTY') ? context.value : r.get('ACKQTY');
                        Ext.each(modifiedRec, function (item) {
                            if (item.crudState === 'U') {
                                Ext.Ajax.request({
                                    url: T2Update,
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: r.get('SEQ'),
                                        ACKQTY: ackqty,
                                        MMCODE: r.get('MMCODE'),
                                        TOWH: r.get('TOWH'),
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            item.ACKQTY = ackqty;
                                            item.commit();
                                            msglabel('資料更新成功');
                                            
                                        }
                                        else {
                                            item.reject();
                                            Ext.Msg.alert('提醒', data.msg, function () {
                                                editor.startEdit(context.rowIdx, 5);
                                            });

                                        }
                                    },
                                    failure: function (response) {
                                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    },
                                    //jsonData: data
                                });
                            }
                        });
                    }
                }
            })
        ]
    });

    function T1Load() {
        T1Tool.moveFirst();
        T1Store.load({
            params: {
                start: 0
            }
        });
    }

    function T2Load() {
        T2Store.load({
            params: {
                p0: Dno,
            }
        });

        
    }

  
    var TWinFormMMCode = Ext.create('WEBAPP.form.MMCodeCombo', {
        name: 'P14',
        fieldLabel: '院內碼',
        labelAlign: 'right',
       // limit: 10, //限制一次最多顯示10筆
        queryUrl: '/api/AB0018/GetMMCodeDocd', //指定查詢的Controller路徑
        extraFields: ['MMNAME_C', 'MMNAME_E', 'BASE_UNIT'], //查詢完會回傳的欄位
        getDefaultParams: function () { //查詢時Controller固定會收到的參數
            return {         
                p1:TWinForm.getForm().findField('P9').getValue() //申請單號 DOCNO
            };
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
            }
        }
    });
    var TWinForm = Ext.widget({
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
                xtype: 'textfield',
                fieldLabel: '申請單號',
                name: 'P9',
                id: 'P9',
                width: 200,
                padding: '0 4 0 4',
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 90
            }, {
                xtype: 'combo',
                fieldLabel: '核撥庫房',
                name: 'P10',
                id: 'P10',
                store: st_frwhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            }, {

                xtype: 'datefield',
                id: 'P11',
                name: 'P11',
                fieldLabel: '申請日期',
                width: 180,
                vtype: 'dateRange',
                dateRange: { end: 'P12' },
                value: getToday()
            }, {
                xtype: 'datefield',
                id: 'P12',
                name: 'P12',
                fieldLabel: '至',
                labelSeparator: '',
                width: 130,
                labelWidth: 20,
                vtype: 'dateRange',
                dateRange: { begin: 'P11' }
            }, {
                xtype: 'combo',
                fieldLabel: '領用庫房',
                name: 'P13',
                id: 'P13',
                store: st_towhcombo,
                queryMode: 'local',
                displayField: 'TEXT',
                valueField: 'VALUE'
            }, TWinFormMMCode
        ],
        buttons: [{
            itemId: 'export', text: '匯出',
            handler: function () {
                var p = new Array();
                p.push({ name: 'FN', value: '查詢點收差異資料.xls' });
                p.push({ name: 'P9', value: TWinForm.getForm().findField('P9').getValue() });    //申請單號
                p.push({ name: 'P10', value: TWinForm.getForm().findField('P10').getValue() });  //核撥庫房
                p.push({ name: 'P11', value: TWinForm.getForm().findField('P11').rawValue });    //申請日期 起
                p.push({ name: 'P12', value: TWinForm.getForm().findField('P12').rawValue });    //申請日期 迄
                p.push({ name: 'P13', value: TWinForm.getForm().findField('P13').getValue() });  //領用庫房
                p.push({ name: 'P14', value: TWinForm.getForm().findField('P14').getValue() });  //院內碼                
                PostForm(T1GetExcel, p);
                msglabel('訊息區:匯出完成');
            },
        }, {
            itemId: 'clean', text: '取消', handler: function () {
                hideWin()
            }
        }]
    });
    function applyDetail() {
        var recs = T2Grid.getSelectionModel().getSelection();//T2Grid.getStore().data.items;

        var procSeq = '';
        for (var i = 0; i < recs.length; i++) {

            if (recs[i].data.POSTID != "C") {
                return;
            }

            if (i == recs.length - 1)
                procSeq += recs[i].data.SEQ + "^" + recs[i].data.ACKQTY + "^" + recs[i].data.TOWH_NO + "^" + recs[i].data.MMCODE;
            else
                procSeq += recs[i].data.SEQ + "^" + recs[i].data.ACKQTY + "^" + recs[i].data.TOWH_NO + "^" + recs[i].data.MMCODE + "ˋ";
        }
        Ext.Ajax.request({

            url: '/api/AB0018/ApplyD',
            method: reqVal_p,
            params: {
                p0: recs[0].data.DOCNO,
                p1: procSeq
            },
            success: function () {
                Ext.Msg.alert('提示', '單筆點收成功');
                //T2Load();
                Ext.Ajax.request({
                    async: false,
                    url: '/api/AB0018/CheckMEDOCM',
                    actionMethods: {
                        read: 'POST' // by default GET
                    },
                    params: {
                        p0: Dno
                    },

                    success: function (response) {
                        var responseText = Ext.decode(response.responseText),
                            clsCK = responseText;
                        if (clsCK.DOCNO == "0") {
                            T1Load();
                            Dno = "";
                        }
                        T2Load();
                    },
                    failure: function () {
                        //Ext.Msg.alert('提示', '儲存失敗');
                    }
                });
            },
            failure: function () {
                alert('fail');
            }
        });
    }

    var callableWin = null;
    popWinForm = function (docno) {
        var strUrl = "AB1801?strParam=" + docno;
        // var strUrl = "AA0038_Form?strVtype=" + vType + "&strMmcode=" + strMmcode;
        if (!callableWin) {
            var popform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                height: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + strUrl + '" id="mainContent" name="mainContent" width="100%" height="100%" frameborder="0"  style="background-color:#FFFFFF"></iframe>',
                buttons: [{
                    id: 'winclosed',
                    disabled: false,
                    text: '關閉',
                    handler: function () {
                        this.up('window').destroy();
                        callableWin = null;
                    }
                }]
            });
            callableWin = GetPopWin(viewport, popform, '藥局領用記錄', viewport.width - 20, viewport.height - 20);
        }
        callableWin.show();
    }

    var win;
    var winActWidth = 300;
    var winActHeight = 250;
    if (!win) {
        win = Ext.widget('window', {
            title: '查詢點收差異',
            closeAction: 'hide',
            width: winActWidth,
            height: winActHeight,
            layout: 'fit',
            resizable: true,
            modal: true,
            constrain: true,
            items: TWinForm,  
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
            win.show();
        }
    }
    function hideWin() {
        if (!win.hidden) {
            win.hide();
        }
    }

    var TATabs = Ext.widget('tabpanel', {
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
            items: [T1QueryForm]

        }]
    });

    var viewport = Ext.create('Ext.Viewport', {
        id: 'viewport',
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
                        height: '50%',
                        items: [T1Grid]
                    },
                    {
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        height: '50%',
                        split: true,
                        //items: [TATabs]
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
            title: '',
            border: false,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [TATabs]
        }
        ]
    });
    
    //T1Load();
    //T1QueryForm.getForm().findField('P7').setValue(st_getlogininfo.getAt(0).get('MR2'));
    //T1QueryForm.getForm().findField('P3').setValue(st_getlogininfo.getAt(0).get('MR3'));

    var arrP8;
    if (user_tflow == '1') {
        arrP8 = ["0102", "0103"];
        T1QueryForm.getForm().findField('P8').setValue(arrP8);
    }
    else if (user_tflow == '6') {
        arrP8 = ["0602", "0603"];
        T1QueryForm.getForm().findField('P8').setValue(arrP8);
    }
    else {
        //
    }
});