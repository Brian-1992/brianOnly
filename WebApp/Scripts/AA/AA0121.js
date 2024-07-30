Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var viewModel = Ext.create('WEBAPP.store.AA.AA0121VM');   // 定義於/Scripts/app/store/AA/AA0121VM.js

    var TaskIdGet = '../../../api/AA0121/GetTaskIdCombo';
    var chkTuserGet = '/api/AA0121/ChkTuser';
    var chkWhNoGet = '/api/AA0121/ChkWhNo';
    var T1Rec = 0;
    var T2Rec = 0;
    var T1aRec = 0;
    var T2aRec = 0;


    var taskIdGridStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });    

    function getTaskIdGridCombo() {
        Ext.Ajax.request({
            url: TaskIdGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var taskIds = data.etts;
                    if (taskIds.length > 0) {
                        //taskIdGridStore.add({ VALUE: '', TEXT: '' });
                        for (var i = 0; i < taskIds.length; i++) {
                            taskIdGridStore.add({ VALUE: taskIds[i].VALUE, TEXT: taskIds[i].TEXT });
                        }
                    }
                }
            }
        })
    }


    //#region 庫房選人員
    // T1: 上方grid
    // T2: 下方grid
    var T1Store = viewModel.getStore('MI_WHID');
    var T2Store = viewModel.getStore('UR_ID');

    var wh_NoCombo = Ext.create('WEBAPP.form.WH_NoCombo', {
        //name: 'WH_NO',
        name: 'WH_ID_CODE',
        id: 'WH_ID_CODE',
        fieldLabel: '庫房代碼',
        fieldCls: 'required',
        allowBlank: false,
        //width: 150,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0059/GetWH_NoCombo',

        //查詢完會回傳的欄位
        extraFields: ['TUSER', 'INID'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            //var f = T2Form.getForm();
            //if (!f.findField("MMCODE").readOnly) {
            //    tmpArray = f.findField("FRWH2").getValue().split(' ');
            //    return {
            //        //MMCODE: f.findField("MMCODE").getValue(),
            //        WH_NO: tmpArray[0]
            //    };
            //}
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                //var f = T2Form.getForm();
                //if (r.get('MMCODE') !== '') {
                //    f.findField("MMCODE").setValue(r.get('MMCODE'));
                //    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                //    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                //    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                //}
            },
            blur: function (field, eOpts) {
                chkWhNo();
            }
        }
    });
    function chkWhNo() {
        var getVal = T1Query.getForm().findField('WH_ID_CODE').getValue();
        if (getVal != "" && getVal != null) {
            Ext.Ajax.request({
                url: chkWhNoGet,
                params: {
                    p0: getVal
                },
                method: reqVal_p,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        var rtnMsg = data.msg;
                        if (rtnMsg == 'N') {
                            Ext.Msg.alert('訊息', getVal + ' 庫房基本資料尚未建檔');
                            T1Query.getForm().findField('WH_ID_CODE').setValue('');
                        }
                    }
                },
                failure: function (response, options) {

                }
            });
        }
    }

    var T1Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 70
        },
        width: 1250,
        height: 23,
        items: [
            wh_NoCombo,
            {
                xtype: 'button',
                text: '查詢',
                id: 'btnQry1',
                //iconCls: 'TRASearch',
                handler: function () {
                    taskIdGridStore.removeAll();
                    getTaskIdGridCombo();
                    T1Load();
                }
            },
            {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    //Ext.getCmp("WH_ID_CODE").setValue('');     // 庫房代碼 clear
                    //Ext.getCmp('INID_NM').setValue('');
                    //T1Store.removeAll();
                    //T1Tool.down('#delete').setDisabled(true);  // enable or disable del button of pagingtool
                    //T2Store.removeAll();
                    //T2Tool.down('#uplddata1').setDisabled(true);
                    //Ext.getCmp('btnQry2').setDisabled(true);
                    //T1Tool.setDisabled(true);
                    //T2Tool.setDisabled(true);
                    var f = this.up('form').getForm();
                    f.reset();
                    taskIdGridStore.removeAll();
                    f.findField('WH_ID_CODE').focus();
                }
            }
        ]
    });
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        //disabled: true,
        buttons: [
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('訊息', '請選擇要刪除的資料');
                        return;
                    }

                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {

                            var list = [];
                            for (var i = 0; i < selection.length; i++) {
                                var temp = {
                                    WH_NO: selection[i].data.WH_NO,
                                    WH_USERID: selection[i].data.WH_USERID,
                                    TASK_ID: selection[i].data.TASK_ID,
                                }
                                list.push(temp);
                            }

                            
                            Ext.Ajax.request({
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                async: false,     // 同步處理
                                url: '/api/AA0121/DeleteM',
                                params: { p0: Ext.util.JSON.encode(list) },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('訊息區:資料刪除成功');
                                        T1Load();

                                        T2Load();
                                    }
                                },
                                failure: function (response, options) {
                                    Ext.Msg.alert('訊息', 'failure!');
                                }
                            });
                        }
                    });
                }
            }
        ]
    });
    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        //cls: 'T1',
        height: '100%',
        dockedItems: [{
            dock: 'top',
            layout: 'fit',
            xtype: 'toolbar',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [
            {
                xtype: 'rownumberer',
                width: 30
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 100,
                sortable: true
            }, {
                text: "人員代碼",
                dataIndex: 'WH_USERID',
                width: 130,
                sortable: false
            },
            {
                text: "人員姓名",
                dataIndex: 'WH_UNA',
                width: 200,
                sortable: false
            },
            {
                text: "<span style='color:red'>作業類別</span>",
                dataIndex: 'TASK_ID',
                itemId: 'T1_COL_TASK_ID',
                flex: 1,
                sortable: false,
                renderer: function (value) {
                    for (var i = 0; i < taskIdGridStore.data.items.length; i++) {
                        if (taskIdGridStore.data.items[i].data.VALUE == value) {
                            return taskIdGridStore.data.items[i].data.TEXT;
                        }
                        else if (value == '1')
                            return '1 藥品';
                    }
                },
                editor: {
                    xtype: 'combobox',
                    store: taskIdGridStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local'
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        listeners: {
            click: {
                //fn: function () {
                //    if (T2Form.hidden === true) {
                //        T1Form.setVisible(false); T2Form.setVisible(true);
                //    }
                //}
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;                           // master grid 有勾選一筆以上的資料
                T1Tool.down('#delete').setDisabled(T1Rec === 0);  // enable or disable del button of pagingtool
            }
        },
        plugins: {
            ptype: 'rowediting',
            clicksToEdit: 1,
            autoCancel: false,
            saveBtnText: '更新',
            cancelBtnText: '取消',
            errorsText: '錯誤訊息',
            dirtyText: '請按更新以修改資料或取消變更',
            listeners: {
                beforeedit: function (editor, context, eOpts) {
                    // 作業類別若是藥品庫則不可修改(預代為1)
                    if (context.record.data['WH_KIND'] == '0')
                        return false;
                },
                edit: function (editor, context, eOpts) {
                    T1Store.load({
                        params: {
                            start: 0
                        }
                    });
                    var data = { item: [] };
                    data.item.push(context.record.data);

                    Ext.Ajax.request({
                        url: '/api/AA0121/UpdateTaskId',
                        method: reqVal_p,

                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('訊息區:資料更新成功');
                                T1Store.load({
                                    params: {
                                        start: 0
                                    }
                                });
                            } else {
                                
                                //Ext.Msg.alert('失敗', 'Ajax communication failed');
                                msglabel('訊息區:資料更新失敗');
                                if (data.msg != '') {
                                    Ext.Msg.alert('失敗', data.msg);
                                } else {
                                    Ext.Msg.alert('失敗', '作業類別錯誤');
                                }
                            }
                        },
                        failure: function (response) {
                            
                            var data = Ext.decode(response.responseText);

                            if (data.msg != '') {
                                Ext.Msg.alert('失敗', data.msg);
                            } else {
                                Ext.Msg.alert('失敗', '作業類別錯誤');
                            }
                        },
                        jsonData: data
                    });
                },
                canceledit: function (editor, context, eOpts) {
                }
            }
        },
    });

    function T1Load() {
        if (!T1Query.getForm().findField('WH_ID_CODE').getValue()) {
            Ext.MessageBox.alert('錯誤', '尚未選取庫房代碼');
        } else {
            T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('WH_ID_CODE').getValue());
            T1Store.getProxy().setExtraParam("p1", "");
            T1Store.getProxy().setExtraParam("p2", "normalByWH");
            T1Store.load({
                params: {
                    start: 0
                }
                ,
                callback: function (records, operation, success) {
                    if (success) {
                        Ext.getCmp('btnQry2').setDisabled(false);     // Detail Qry button enable
                        if (records.length == 0) {
                            //Ext.Msg.alert('沒有符合的資料!')
                        } else {
                            if (records.length > 0) {
                                T1Tool.moveFirst();
                                T1Tool.setDisabled(false);
                            }
                        }
                    } else {
                        Ext.getCmp('btnQry2').setDisabled(true);
                        Ext.Msg.alert('訊息', 'failure!');
                    }
                }
            })
            // 若已有查詢的人員資料,則重新查詢
            if (T2Store.count() > 0)
                T2Load();
        }

    }

    var T2Query = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 90
        },
        width: 1250,
        height: 23,
        items: [
            {
                fieldLabel: '責任中心',
                name: 'INID_NM',
                id: 'INID_NM',
                labelWidth: 60,
                width: 170,
                enforceMaxLength: true,
                maxLength: 50,
                padding: '0 4 0 4'
            }, {
                fieldLabel: '人員姓名',
                name: 'WH_UNA',
                id: 'WH_UNA',
                labelWidth: 60,
                width: 170,
                enforceMaxLength: true,
                maxLength: 50,
                padding: '0 4 0 4'
            }, {
                xtype: 'button',
                text: '查詢',
                id: 'btnQry2',
                disabled: true,
                handler: T2Load
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    ////var f = this.up('form').getForm();
                    ////f.reset();
                    ////f.findField('INID_ID').focus();
                    //Ext.getCmp('INID_NM').setValue('');
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('INID_NM').focus();
                }
            }
        ]
    });
    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        //disabled: true,
        buttons: [
            {
                itemId: 'uplddata1', text: '指定', disabled: true,
                handler: function () {
                    if (!T1Query.getForm().findField('WH_ID_CODE').getValue()) {
                        Ext.MessageBox.alert('錯誤', '尚未選取庫房代碼');
                        return;
                    }

                    var selection = T2Grid.getSelection();
                    if (selection.length == 0) {
                        Ext.MessageBox.alert('提示', '請選擇人員');
                        return;
                    }

                    Ext.MessageBox.confirm('指定', '是否要指定人員？', function (btn, text) {
                        if (btn === 'yes') {
                            var list = [];
                            
                            for (var i = 0; i < selection.length; i++) {
                                var temp = {
                                    WH_NO: T1Query.getForm().findField('WH_ID_CODE').getValue(),
                                    WH_USERID: selection[i].data.TUSER
                                };
                                list.push(temp);
                            }

                            Ext.Ajax.request({
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                url: '/api/AA0121/InsertM',
                                params: { p0: Ext.util.JSON.encode(list) },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('訊息區:人員新增完成');

                                        T1Load();

                                        T2Load();
                                    } else {
                                        Ext.Msg.alert('錯誤', data.msg);
                                    }
                                }
                            });
                            /////alert("ppp");
                        }
                    }
                    );
                }
            }
        ]
    });
    var T2Grid = Ext.create('Ext.grid.Panel', {
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        //cls: 'T1',
        height: '100%',
        dockedItems: [{
            dock: 'top',
            layout: 'fit',
            xtype: 'toolbar',
            items: [T2Query]
        }
            , {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2Tool]
        }
        ],
        columns: [
            {
                xtype: 'rownumberer',
                width: 30
            },
            {
                text: "人員代碼",
                dataIndex: 'TUSER',
                width: 100,
                sortable: true
            }, {
                text: "人員姓名",
                dataIndex: 'UNA',
                width: 180,
                sortable: true
            }, {
                text: "英文姓名",
                dataIndex: 'UENA',
                width: 100,
                sortable: true
            }, {
                text: "責任中心",
                dataIndex: 'INID',

                flex: 1,
                sortable: true
            },
            {
                header: "",
                flex: 1
            }
        ],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        listeners: {
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2Tool.down('#uplddata1').setDisabled(T2Rec === 0);  // enable or disable del button of pagingtool
            }
        }
    });

    function T2Load() {
        ///if (T2Query.getForm().findField('INID_NM').getValue() == "") {
        ///    Ext.Msg.alert("責任中心，不可空白!");
        ///} else {
        T2Store.getProxy().setExtraParam("p0", T2Query.getForm().findField('INID_NM').getValue());    // 責任中心
        T2Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('WH_ID_CODE').getValue()); // 庫房代碼
        T2Store.getProxy().setExtraParam("p2", 'normalByWH');   // flag : normalByWH
        T2Store.getProxy().setExtraParam("p4", T2Query.getForm().findField('WH_UNA').getValue());    // 人員姓名
        T2Store.load({
            params: {
                start: 0
            },
            callback: function (records, operation, success) {
                if (success) {
                    //Ext.getCmp('btnQry2').setDisabled(false);     // Detail Qry button enable
                    if (records.length == 0) {
                        //Ext.Msg.alert('沒有符合的資料!')
                        msglabel('訊息區:沒有符合的資料');
                    } else {
                        if (records.length > 0) {
                            T2Tool.moveFirst();
                            T2Tool.setDisabled(false);
                        }
                    }
                } else {
                    Ext.getCmp('btnQry2').setDisabled(true);
                    Ext.Msg.alert('訊息', 'failure!');
                    msglabel('訊息區:failure');
                }
            }
        })
        ///}
    }
    function T2Cleanup() {
        viewport.down('#t1Grid').unmask();
        var f = T2Form.getForm();
        f.reset();
        f.findField('DATA_SEQ').setReadOnly(true);
        f.findField('DATA_NAME').setReadOnly(true);
        f.findField('DATA_VALUE').setReadOnly(true);
        f.findField('DATA_DESC').setReadOnly(true);
        f.findField('DATA_REMARK').setReadOnly(true);
        T2Form.down('#T2Cancel').hide();
        T2Form.down('#T2Submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        setFormT2a();
    }

    var vboxPanel1a = new Ext.Panel({
        layout: {
            type: 'vbox',
            align: 'stretch' //拉伸使其充滿整個父容器
        },
        items: [
            T1Grid
        ]
    });
    var vboxPanel1b = new Ext.Panel({
        layout: {
            type: 'vbox',
            align: 'stretch' //拉伸使其充滿整個父容器
        },
        items: [T2Grid]
    });


    //#endregion

    //#region 人員選庫房
    // T1a: 上方grid
    // T2a: 下方grid
    var T1aStore = viewModel.getStore('MI_WHIDa');
    var T2aStore = viewModel.getStore('MI_WHMAST');


    var usrNameCombo = Ext.create('WEBAPP.form.UR_IDNameCombo', {
        name: 'USER_ID_NAME',
        id: 'USER_ID_NAME',
        fieldLabel: '人員姓名',
        fieldCls: 'required',
        allowBlank: false,

        //限制一次最多顯示10筆
        limit: 10,

        //指定查詢的Controller路徑
        queryUrl: '/api/AA0121/GetUsrNameCombo',

        //查詢完會回傳的欄位
        extraFields: ['UNA', 'UENA'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
        },
        listeners: {
            select: function (c, r, i, e) {

            },
            blur: function (field, eOpts) {
                chkTuser();
            }
        }
    });
    function chkTuser() {
        var getVal = T1aQuery.getForm().findField('USER_ID_NAME').getValue();
        if (getVal != '' && getVal != null) {
            Ext.Ajax.request({
                url: chkTuserGet,
                params: {
                    p0: getVal
                },
                method: reqVal_p,
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        var rtnMsg = data.msg;
                        if (rtnMsg == 'N') {
                            Ext.Msg.alert('訊息', getVal + ' 使用者基本資料尚未建檔');
                            T1aQuery.getForm().findField('USER_ID_NAME').setValue('');
                        }
                    }
                },
                failure: function (response, options) {

                }
            });
        }
    }

    var T1aQuery = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelAlign: "right",
            labelWidth: 70
        },
        width: 1250,
        height: 23,
        items: [
            usrNameCombo,
           {
                xtype: 'button',
                text: '查詢',
                id: 'btnQry1a',
                //iconCls: 'TRASearch',
                disabled: false,
                handler: function () {
                    taskIdGridStore.removeAll();
                    getTaskIdGridCombo();
                    T1aLoad();
                }
            }
            , {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    //Ext.getCmp("USER_ID_CODE").setValue('');   // 人員代碼 clear
                    ////Ext.getCmp('INID_CODE').setValue('');
                    //T1aStore.removeAll();
                    //T1aTool.down('#delete').setDisabled(true);  // enable or disable del button of pagingtool

                    //T2aStore.removeAll();
                    //T2aTool.down('#uplddata2').setDisabled(true);
                    //Ext.getCmp('btnQry2a').setDisabled(true);
                    ////T1aTool.setDisabled(true);
                    ////T2aTool.setDisabled(true);

                    //////////Ext.getCmp('btnQry1a').setDisabled(true);
                    var f = this.up('form').getForm();
                    f.reset();
                    taskIdGridStore.removeAll();
                    f.findField('USER_ID_NAME').focus();

                }
            }

        ]
    });
    var T1aTool = Ext.create('Ext.PagingToolbar', {
        store: T1aStore,
        displayInfo: true,
        border: false,
        plain: true,
        //disabled: true,
        buttons: [
            {
                itemId: 'delete', text: '刪除', disabled: true,
                handler: function () {
                    var selection = T1aGrid.getSelection();
                    if (selection.length == 0) {
                        Ext.Msg.alert('訊息', '請選擇要刪除的資料');
                        return;
                    }

                    Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
                        if (btn === 'yes') {

                            var list = [];
                            for (var i = 0; i < selection.length; i++) {
                                var temp = {
                                    WH_NO: selection[i].data.WH_NO,
                                    WH_USERID: selection[i].data.WH_USERID,
                                    TASK_ID: selection[i].data.TASK_ID,
                                }
                                list.push(temp);
                            }

                            
                            Ext.Ajax.request({
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                async: false,     // 同步處理
                                url: '/api/AA0121/DeleteM',
                                params: { p0: Ext.util.JSON.encode(list) },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('訊息區:資料刪除成功');
                                        T1aLoad();

                                        T2aLoad();
                                    }
                                },
                                failure: function (response, options) {
                                    Ext.Msg.alert('訊息', 'failure!');
                                }
                            });
                        }
                    });
                }
            }
        ]
    });
    var T1aGrid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
        store: T1aStore,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        //cls: 'T1',
        height: '100%',
        dockedItems: [{
            dock: 'top',
            xtype: 'toolbar',
            layout: 'fit',
            items: [T1aQuery]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1aTool]
        }
        ],
        columns: [
            {
                xtype: 'rownumberer',
                width: 30
            },
            {
                text: "人員代碼",
                dataIndex: 'WH_USERID',
                width: 100,
                sortable: false
            }, {
                text: "人員姓名",
                dataIndex: 'WH_UNA',
                width: 180,
                sortable: false
            }, {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 100,
                sortable: false
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                width: 180,
                sortable: false
            }, {
                text: "<span style='color:red'>作業類別</span>",
                dataIndex: 'TASK_ID',
                itemId: 'T1a_COL_TASK_ID',
                flex: 1,
                sortable: false,
                renderer: function (value) {
                    for (var i = 0; i < taskIdGridStore.data.items.length; i++) {
                        if (taskIdGridStore.data.items[i].data.VALUE == value) {
                            return taskIdGridStore.data.items[i].data.TEXT;
                        }
                        else if (value == '1')
                            return '1 藥品';
                    }
                },
                editor: {
                    xtype: 'combobox',
                    store: taskIdGridStore,
                    displayField: 'TEXT',
                    valueField: 'VALUE',
                    queryMode: 'local'
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        }),
        listeners: {
            click: {
                //fn: function () {
                //    if (T2Form.hidden === true) {
                //        T1Form.setVisible(false); T2Form.setVisible(true);
                //    }
                //}
            },
            selectionchange: function (model, records) {
                T1aRec = records.length;                           // master grid 有勾選一筆以上的資料
                T1aTool.down('#delete').setDisabled(T1aRec === 0);  // enable or disable del button of pagingtool
            }
        },
        plugins: {
            ptype: 'rowediting',
            clicksToEdit: 1,
            autoCancel: false,
            saveBtnText: '更新',
            cancelBtnText: '取消',
            errorsText: '錯誤訊息',
            dirtyText: '請按更新以修改資料或取消變更',
            listeners: {
                beforeedit: function (editor, context, eOpts) {
                    // 作業類別若是藥品庫則不可修改(預代為1)
                    if (context.record.data['WH_KIND'] == '0')
                        return false;
                },
                edit: function (editor, context, eOpts) {
                    T1aStore.load({
                        params: {
                            start: 0
                        }
                    });
                    var data = { item: [] };
                    data.item.push(context.record.data);

                    Ext.Ajax.request({
                        url: '/api/AA0121/UpdateTaskId',
                        method: reqVal_p,

                        success: function (response) {
                            var data = Ext.decode(response.responseText);
                            if (data.success) {
                                msglabel('訊息區:資料更新成功');
                                T1aStore.load({
                                    params: {
                                        start: 0
                                    }
                                });
                            } else {
                                //Ext.Msg.alert('失敗', 'Ajax communication failed');
                                msglabel('訊息區:資料更新失敗');
                                Ext.Msg.alert('失敗', '作業類別錯誤');
                            }
                        },
                        failure: function (response) {
                            Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                        },
                        jsonData: data
                    });
                },
                canceledit: function (editor, context, eOpts) {
                }
            }
        },
    });

    function T1aLoad() {
        if (!T1aQuery.getForm().findField('USER_ID_NAME').getValue()) {
            Ext.MessageBox.alert('錯誤', '尚未選取人員姓名');
            //Ext.Msg.alert("人員代碼，不可空白!");
        } else {

            T1aStore.getProxy().setExtraParam("p0", T1aQuery.getForm().findField('USER_ID_NAME').getValue());
            //T1aStore.getProxy().setExtraParam("p1", T1aQuery.getForm().findField('INID_CODE').getValue());
            T1aStore.getProxy().setExtraParam("p2", "normalByUser");
            T1aStore.load({
                params: {
                    start: 0,
                    page: 1
                },
                callback: function (records, operation, success) {
                    if (success) {
                        Ext.getCmp('btnQry2a').setDisabled(false);     // tab2 Detail Qry button enable
                        if (records.length == 0) {
                            //Ext.Msg.alert('沒有符合的資料!')
                        } else {
                            if (records.length > 0) {
                                T1aTool.setDisabled(false);
                            }
                        }
                    } else {
                        Ext.getCmp('btnQry2a').setDisabled(true);
                        Ext.Msg.alert('訊息', 'failure!');
                    }
                }

            })
            //if (T2aStore.count() > 0)
            //    T2aLoad();
        }

    }


    var T2aQuery = Ext.widget({
        xtype: 'form',
        layout: 'hbox',
        border: false,
        defaultType: 'textfield',
        fieldDefaults: {
            labelWidth: 90
        },
        width: 1250,
        height: 23,
        items: [
            {
                fieldLabel: '庫房代碼',
                name: 'WH_CODE',
                id: 'WH_CODE',
                labelWidth: 60,
                width: 170,
                enforceMaxLength: true,
                maxLength: 50,
                padding: '0 4 0 4'
            }, {
                fieldLabel: '庫房名稱',
                name: 'WH_NAME',
                id: 'WH_NAME',
                labelWidth: 60,
                width: 170,
                enforceMaxLength: true,
                maxLength: 50,
                padding: '0 4 0 4'
            }, {
                xtype: 'button',
                text: '查詢',
                id: 'btnQry2a',
                disabled: true,
                handler: T2aLoad
            }, {
                xtype: 'button',
                text: '清除',
                handler: function () {
                    //var f = this.up('form').getForm();
                    //f.reset();
                    //f.findField('WH_CODE').focus();
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('WH_CODE').focus();
                }
            }
        ]
    });
    var T2aTool = Ext.create('Ext.PagingToolbar', {
        store: T2aStore,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                itemId: 'uplddata2', text: '指定', disabled: true,
                handler: function () {
                    if (!T1aQuery.getForm().findField('USER_ID_NAME').getValue()) {
                        Ext.MessageBox.alert('錯誤', '尚未選取人員姓名');
                        return;
                    }
                    var selection = T2aGrid.getSelection();
                    if (selection.length == 0) {
                        Ext.MessageBox.alert('提示', '尚未選取庫房');
                        return;
                    }

                    Ext.MessageBox.confirm('指定', '是否要指定庫房？', function (btn, text) {
                        if (btn === 'yes') {
                            var list = [];
                            
                            for (var i = 0; i < selection.length; i++) {
                                var temp = {
                                    WH_NO: selection[i].data.WH_NO,
                                    WH_USERID: T1aQuery.getForm().findField('USER_ID_NAME').getValue()
                                };
                                list.push(temp);
                            }

                            Ext.Ajax.request({
                                actionMethods: {
                                    read: 'POST' // by default GET
                                },
                                url: '/api/AA0121/InsertM',
                                params: { p0: Ext.util.JSON.encode(list) },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('庫房新增完成');

                                        T1aLoad();

                                        T2aLoad();
                                    } else {
                                        Ext.Msg.alert('錯誤', data.msg);
                                    }
                                }
                            });
                        }
                    }
                    );
                }
            }
        ]
    });
    var T2aGrid = Ext.create('Ext.grid.Panel', {
        store: T2aStore,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        //cls: 'T1',
        height: '100%',
        dockedItems: [{
            dock: 'top',
            layout: 'fit',
            xtype: 'toolbar',
            items: [T2aQuery]
        }
            , {
            dock: 'top',
            xtype: 'toolbar',
            items: [T2aTool]
        }
        ],
        columns: [
            {
                xtype: 'rownumberer',
                width: 30
            },
            {
                text: "庫房代碼",
                dataIndex: 'WH_NO',
                width: 100,
                sortable: true
            }, {
                text: "庫房名稱",
                dataIndex: 'WH_NAME',
                flex: 1,
                sortable: true
            }, {
                text: "庫房分類",
                dataIndex: 'WH_KIND',
                flex: 1,
                sortable: true,
                renderer: function (value) {
                    if (value == '0')
                        return "藥品庫";
                    else if (value == '1')
                        return "衛材庫";
                    else if (value == '2')
                        return "戰備庫";
                    else if (value == '3')
                        return "疾管局庫";
                }
            }, {
                text: "庫房級別",
                dataIndex: 'WH_GRADE',
                flex: 1,
                sortable: true,
                renderer: function (value) {
                    if (value == '1')
                        return "庫";
                    else if (value == '2')
                        return "局(衛星庫)";
                    else if (value == '3')
                        return "病房";
                    else if (value == '4')
                        return "科室";
                    else if (value == '5')
                        return "戰備庫";
                    else if (value == '6')
                        return "診間";
                    else if (value == 'X')
                        return "其它";
                }
            },
            {
                header: "",
                flex: 1
            }
        ],
        //selModel: checkboxT1Model2a,
        listeners: {
            click: {
                element: 'el',
                fn: function () {
                    //if (T2Form.hidden === true) {
                    //    T1Form.setVisible(false); T2Form.setVisible(true);
                    //}
                }
            },
            selectionchange: function (model, records) {
                //T2aRec = records.length;
                //T2aLastRec = records[0];
                T2aRec = records.length;
                T2aTool.down('#uplddata2').setDisabled(T2aRec === 0);  // enable or disable del button of pagingtool
            }
        },
        selModel: Ext.create('Ext.selection.CheckboxModel', {//根據條件disable checkbox
            checkOnly: false,
            injectCheckbox: 'first',
            mode: 'MULTI',
            selType: 'checkboxmodel',
            showHeaderCheckbox: true,
            selectable: function (record) {
                return false;
            }
        })
    });

    function T2aLoad() {
        //if (T2aQuery.getForm().findField('WH_CODE').getValue() == "") {
        //    Ext.Msg.alert("庫房代碼，不可空白!");
        //    msglabel('訊息區:庫房代碼，不可空白');
        //} else {
        T2aStore.getProxy().setExtraParam("p0", T2aQuery.getForm().findField('WH_CODE').getValue());        // 庫房代碼
        T2aStore.getProxy().setExtraParam("p1", T1aQuery.getForm().findField('USER_ID_NAME').getValue());   // 人員姓名
        T2aStore.getProxy().setExtraParam("p2", 'normalByUser'); // FLAG
        //T2aStore.getProxy().setExtraParam("p3", T1aQuery.getForm().findField('INID_CODE').getValue());      // 責任中心
        T2aStore.getProxy().setExtraParam("p4", T2aQuery.getForm().findField('WH_NAME').getValue());        // 庫房名稱
        T2aStore.load({
            params: {
                start: 0,
                page:1
            },
            callback: function (records, operation, success) {
                if (success) {
                    //Ext.getCmp('btnQry2').setDisabled(false);     // Detail Qry button enable
                    if (records.length == 0) {
                        //Ext.Msg.alert('沒有符合的資料!')
                        msglabel('訊息區:沒有符合的資料');
                    } else {
                        if (records.length > 0) {
                            T2aTool.moveFirst();
                            T2aTool.setDisabled(false);
                        }
                    }
                } else {
                    Ext.getCmp('btnQry2').setDisabled(true);
                    Ext.Msg.alert('訊息', 'failure!');
                    msglabel('訊息區:failure');
                }
            }
        })
        //}
    }

    var vboxPanel2a = new Ext.Panel({
        layout: {
            type: 'vbox',
            align: 'stretch' //拉伸使其充滿整個父容器
        },
        items: [
            T1aGrid
        ]
    });

    var vboxPanel2b = new Ext.Panel({
        layout: {
            type: 'vbox',
            align: 'stretch' //拉伸使其充滿整個父容器
        },
        items: [T2aGrid]
    });
    //#endregion

    //view 
    var TATabs = Ext.widget('tabpanel', {
        plain: true,
        border: false,
        resizeTabs: true,
        //enableTabScroll: false,
        layout: 'fit',
        //autoScroll: false,
        //frame:true,
        defaults: {
            layout: 'fit',
            split: true
        },
        items: [{
            title: '庫房選人員',
            id: 'tab1',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                region: 'center',
                layout: 'fit',
                split: true,
                collapsible: false,
                border: false,
                height: '50%',
                items: [vboxPanel1a]
            }, {
                title: '人員資料',
                region: 'south',
                layout: 'fit',
                split: true,
                collapsible: true,
                border: false,
                height: '50%',
                items: [vboxPanel1b]
            }]
        }, {
            title: '人員選庫房',
            id: 'tab2',
            layout: 'border',
            padding: 0,
            split: true,
            items: [{
                region: 'center',
                layout: 'fit',
                split: true,
                collapsible: false,
                border: false,
                height: '50%',
                items: [vboxPanel2a]
            }, {
                title: '庫房資料',
                region: 'south',
                layout: 'fit',
                split: true,
                collapsible: true,
                border: false,
                height: '50%',
                items: [vboxPanel2b]
            }]
        },],

        listeners: {
            'tabchange': function (tabPanel, tab) {

                if (tab.id == 'tab1') {
                    if (T1Query.getForm().findField('WH_ID_CODE').getValue()) {
                        T1Load();
                    }

                } else {
                    if (T1aQuery.getForm().findField('USER_ID_NAME').getValue()) {
                        T1sLoad();
                    }
                }
            }
        }
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
                        region: 'center',
                        layout: 'fit',
                        collapsible: false,
                        title: '',
                        split: true,
                        height: '100%',
                        items: [TATabs]
                    }
                ]
            }]
        }
        ]
    });

    T1Query.getForm().findField('WH_ID_CODE').focus();

});
