﻿Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Name = '繳回維護';

    var arrP2 = ["1"];
    var arrP3 = ["03", "04", "05", "06", "07"];
    var reportUrl = '/Report/A/AA0062.aspx';
    var userId, userName, userInid, userInidName, isGrade, userTaskId;

    var viewModel = Ext.create('WEBAPP.store.AB.AB0021VM');
    var T1Store = viewModel.getStore('ME_DOCM');
    function T1Load() {
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());

        T1Store.getProxy().setExtraParam("TASKID", userTaskId);
        T1Store.getProxy().setExtraParam("APPDEPT", userInid);
        T1Tool.moveFirst();
        //T1Store.loadPage(1);
        //T1Store.load({
        //    params: {
        //        start: 0
        //    }
        //});
    }

    var T2Store = viewModel.getStore('ME_DOCD');
    function T2Load() {
        if (T1LastRec != null && T1LastRec.data["DOCNO"] !== '') {
            var tmpArray = T1LastRec.data["FRWH"].split(' ');
            var tmpArray1 = T1LastRec.data["TOWH"].split(' ');
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            T2Store.getProxy().setExtraParam("FRWH", tmpArray[0]);
            T2Store.getProxy().setExtraParam("TOWH", tmpArray1[0]);
            T2Store.load({
                params: {
                    start: 0
                }
            });
        }
    }

    var DocnoStore = viewModel.getStore('DOCNO');
    function DocnoLoad() {
        DocnoStore.getProxy().setExtraParam("APPDEPT", session['Inid']);
        DocnoStore.load({
            params: {
                start: 0
            }
        });
    }

    var FlowIdStore = viewModel.getStore('FLOWID');
    function FlowIdLoad() {
        FlowIdStore.load({
            params: {
                start: 0
            }
        });
    }

    var MatClassStore = viewModel.getStore('MAT_CLASS');
    function MatClassLoad() {
        MatClassStore.load({
            params: {
                start: 0
            }
        });
    }

    var st_frwhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0021/GetFrwhCombo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });

    var st_getlogininfo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0021/GetLoginInfo',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var UserInfoStore = viewModel.getStore('USER_INFO');
    function UserInfoLoad() {
        UserInfoStore.load(function (records, operation, success) {
            if (success) {
                var r = records[0];
                userId = r.get('TUSER');
                userName = r.get('UNA');
                userInid = r.get('INID');
                userInidName = r.get('INID_NAME');

                // 檢查是申請人員或庫房人員
                Ext.Ajax.request({
                    url: '/api/AB0021/IsWhGrade1',
                    method: reqVal_p,
                    params: {
                        INID: userInid
                    },
                    success: function (response) {
                        var data = Ext.decode(response.responseText);
                        if (data.success) {
                            // 庫房人員
                            isGrade = true;
                            Ext.getCmp('btnAdd').setVisible(false);
                            Ext.getCmp('btnUpdate').setVisible(false);
                            Ext.getCmp('btnDel').setVisible(false);
                            Ext.getCmp('btnSubmit').setVisible(false);
                            Ext.getCmp('btnExp').setVisible(false);

                            Ext.getCmp('btnAdd2').setVisible(false);
                            Ext.getCmp('btnUpdate2').setVisible(false);
                            Ext.getCmp('btnDel2').setVisible(false);

                            // 取得庫房人員的task id
                            Ext.Ajax.request({
                                url: '/api/AB0021/GetWhTaskId',
                                method: reqVal_p,
                                params: {
                                    USERID: userId
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    userTaskId = data.etts[0].TASK_ID;  // 1-藥品, 2-衛材, 3-一般物品
                                }
                            });
                            Ext.Ajax.setTimeout(600000);
                            arrP2 = ["2"];
                            T1Query.getForm().findField('P2').setValue(arrP2);    // FLOWID = 2
                            
                        }
                        else {
                            // 申請人員
                            isGrade = false;
                            Ext.getCmp('btnReject').setVisible(false);
                            Ext.getCmp('btnSubmit2').setVisible(false);
                            arrP2 = ["1"];
                            T1Query.getForm().findField('P2').setValue(arrP2);    // FLOWID = 1
                        }

                    },
                    failure: function (response) {
                        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                    }
                });
            }
        });
    }

    // 查詢欄位
    var mLabelWidth = 90;
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
        items: [
            {
                xtype: 'combo',
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
                //readOnly: true,
                anyMatch: true,
                typeAhead: true,
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
            },
            {
                //xtype: 'textfield',
                xtype: 'datefield',
                fieldLabel: '繳回日期',
                name: 'D0',
                id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'datefield',
                fieldLabel: '至',
                labelWidth: '10px',
                name: 'D1',
                id: 'D1',
                labelSeparator: '',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4'
            },
            {
                xtype: 'combo',
                fieldLabel: '繳回單狀態',
                name: 'P2',
                id: 'P2',
                store: FlowIdStore,
                labelWidth: 90,
                editable: false,
                queryMode: 'local',
                multiSelect: true,
                displayField: 'COMBITEM',
                valueField: 'VALUE'
            },
            {
                xtype: 'combo',
                fieldLabel: '物料分類',
                name: 'P3',
                id: 'P3',
                store: MatClassStore,
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
                    Ext.getCmp('btnExp').setVisible(true);
                    Ext.getCmp('btnSubmit2').setDisabled(true);
                    Ext.getCmp('btnReject').setDisabled(true);
                }
            },
            {
                itemId: 'clean',
                text: '清除',
                handler: function () {
                    var f = this.up('form').getForm();
                    f.reset();
                    f.findField('P1').focus(); // 進入畫面時輸入游標預設在P0
                    msglabel('訊息區:');

                    T1Query.getForm().findField('P2').setValue(arrP2);
                    setT1Matclassdefault();
                }
            }
        ]
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
                    T1Set = '/api/AB0021/MasterCreate';
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
                    T1Set = '/api/AB0021/MasterUpdate';
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
                        //selection.map(item => {
                        //    name += '「' + item.get('DOCNO') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            name += '「' + item.get('DOCNO') + '」<br>';
                            docno += item.get('DOCNO') + ',';
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除繳回單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0021/MasterDelete',
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
                                            Ext.getCmp('btnExp').setVisible(true);
                                            Ext.getCmp('btnSubmit2').setDisabled(true);
                                            Ext.getCmp('btnReject').setDisabled(true);

                                            T1Cleanup();

                                            T1Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
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
                text: '送繳回',
                id: 'btnSubmit',
                name: 'btnSubmit',
                handler: function () {
                    //if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                    //    Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                    //    return;
                    //}
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
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

                        Ext.MessageBox.confirm('訊息', '確認是否繳回?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0021/UpdateStatus',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        //FLOWID: '2'
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
                                            Ext.getCmp('btnExp').setVisible(true);
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
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                text: '退回',
                id: 'btnReject',
                name: 'btnReject',
                handler: function () {
                    T1Form.setVisible(true);
                    T2Form.setVisible(false);
                    T1Set = '/api/AB0021/MasterReject';
                    Ext.MessageBox.alert('訊息', '請將退回原因填寫於備註欄');
                    setFormT1('R', '退回');
                    T2Store.removeAll();
                    T1Grid.getSelectionModel().deselectAll();
                }
            },
            {
                text: '繳回確認',
                id: 'btnSubmit2',
                name: 'btnSubmit2',
                handler: function () {
                    //Ext.MessageBox.alert('訊息', 'store procedure功能建置中');
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
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

                        Ext.MessageBox.confirm('訊息', '確認是否繳回?單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0021/UpdateStatusBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
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
                                            Ext.getCmp('btnExp').setVisible(true);
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
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                itemId: 'btnExp',
                id: 'btnExp',
                text: '效期',
                disabled: true,
                handler: function () {
                    showExpWindow(T1LastRec.data["DOCNO"], 'O', viewport);
                }
            }
            //,
            //{
            //    itemId: 'print', text: '列印', handler: function () {
            //        //if (T1Store.getCount() > 0) {
            //        reportUrl = '/Report/A/AA0064.aspx';
            //        showReport();
            //        //}
            //        //else
            //        //    Ext.Msg.alert('訊息', '請先輸入查詢條件');
            //    }
            //}
        ]
    });

    function showReport() {
        if (!win) {
            var winform = Ext.create('Ext.form.Panel', {
                id: 'iframeReport',
                //height: '100%',
                //width: '100%',
                layout: 'fit',
                closable: false,
                html: '<iframe src="' + reportUrl + '?type=' + "AA0064"
                + '&APPTIME_START=' + T1Query.getForm().findField('D0').getValue()
                + '&APPTIME_END=' + T1Query.getForm().findField('D1').getValue()
                + '&MAT_CLASS=' + T1Query.getForm().findField('P3').getValue()
                + '&FRWH=' + session['Inid']
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
    // 按'新增'或'修改'時的動作
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
            u = f.findField("MAT_CLASS");
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('MAT_CLASS').setReadOnly(false);
            f.findField('FRWH').setReadOnly(false);
            if (st_frwhcombo.data.length > 0) {
                f.findField('FRWH').setValue(st_frwhcombo.getAt(0).get('VALUE'));
            }
            f.findField("DOCNO").setValue('系統自編');
            f.findField("APPID").setValue(userId + ' ' + userName);
            f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);
            f.findField("APPTIME").setValue(getChtToday());
            var center_wh = st_getlogininfo.getAt(0).get('CENTER_WHNO') + ' ' + st_getlogininfo.getAt(0).get('CENTER_WHNAME');
            f.findField('TOWH').setValue(center_wh);
        }
        else {
            u = f.findField('MAT_CLASS');
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

            //f.findField("MAT_CLASS").setValue(T1LastRec.data["MAT_CLASS"]);
            f.findField("APPLY_NOTE").setValue(T1LastRec.data["APPLY_NOTE"]);
        }
        f.findField('x').setValue(x);
        //f.findField('MAT_CLASS').setReadOnly(false);
        f.findField('APPLY_NOTE').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        u.focus();
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
            //{
            //    xtype: 'displayfield',
            //    fieldLabel: '出庫庫房',
            //    name: 'FRWH',
            //    enforceMaxLength: true,
            //    maxLength: 100,
            //    readOnly: true,
            //    submitValue: true
            //},
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
                queryMode: 'local',
                triggerAction: 'all',
                fieldCls: 'required'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '入庫庫房',
                name: 'TOWH',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
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
                //id: 'MAT_CLASS',
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
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                        //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                        //else {
                        //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        //        if (btn === 'yes') {
                        //            T1Submit();
                        //        }
                        //    });
                        //}

                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                                //T1Set = '';
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

    var T1Grid = Ext.create('Ext.grid.Panel', {
        //title: T1Name,
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
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 100
            },
            {
                text: "物料分類",
                dataIndex: 'MAT_CLASS',
                width: 120
            },
            {
                text: "繳回人員",
                dataIndex: 'APPID',
                width: 100
            },
            {
                xtype: 'hidden',
                text: "繳回人員",
                dataIndex: 'APP_NAME',
                width: 100
            },
            {
                text: "繳回單位",
                dataIndex: 'APPDEPT_NAME',
                width: 200
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
                text: "繳回日期",
                dataIndex: 'APPTIME',
                width: 120
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
                        Ext.getCmp('btnExp').setDisabled(false);

                        var tmpArray = T1LastRec.data["FLOWID"].split(' ');
                        if (isGrade) {  // 庫房人員
                            if (tmpArray[0] === '1' || tmpArray[0] === '3' || tmpArray[0] === 'X') {   // 1-繳回申請中, 3-已繳回, X-已退回
                                Ext.getCmp('btnReject').setDisabled(true);
                                Ext.getCmp('btnSubmit2').setDisabled(true);
                            }
                            else {
                                Ext.getCmp('btnReject').setDisabled(false);
                                Ext.getCmp('btnSubmit2').setDisabled(false);
                            }

                        }
                        else {  // 申請人員
                            if (tmpArray[0] === '2' || tmpArray[0] === '3') {   // 2-繳回中, 3-已繳回
                                Ext.getCmp('btnUpdate').setDisabled(true);
                                Ext.getCmp('btnUpdate2').setDisabled(true);
                                Ext.getCmp('btnDel').setDisabled(true);
                                Ext.getCmp('btnDel2').setDisabled(true);
                                Ext.getCmp('btnAdd2').setDisabled(true);
                                Ext.getCmp('btnSubmit').setDisabled(true);
                            }
                            else {
                                Ext.getCmp('btnUpdate').setDisabled(false);
                                Ext.getCmp('btnDel').setDisabled(false);

                                Ext.getCmp('btnUpdate2').setDisabled(true);
                                Ext.getCmp('btnDel2').setDisabled(true);
                                Ext.getCmp('btnAdd2').setDisabled(false);
                                Ext.getCmp('btnSubmit').setDisabled(false);
                            }
                        }

                        if (T1Set === '')
                            setFormT1a();

                        T2Load();
                    }

                }
            },
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                setFormT1a();
                T2Load();

                if (T1LastRec == null)
                    Ext.getCmp('btnAdd2').setDisabled(true);
            }
        }
    });

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
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
            //f.findField("FRWH").setValue(T1LastRec.data["FRWH"]);
            f.findField("TOWH").setValue(T1LastRec.data["TOWH"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);
            f.findField("APPLY_NOTE").setValue(T1LastRec.data["APPLY_NOTE"]);
            var tmpArray = T1LastRec.data["MAT_CLASS"].split(" ");
            f.findField('MAT_CLASS').setValue(tmpArray[0]);

            f.findField('MAT_CLASS').setReadOnly(true);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);

            var f2 = T2Form.getForm();
            var tmpArray2 = T1LastRec.data["FRWH"].split(' ');
            f2.findField('FRWH1').setValue(tmpArray2[0]);
        }

    }

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            //{
            //    text: '查詢',
            //    handler: function () {
            //        Ext.getCmp('eastform').expand();
            //        T1Form.setVisible(false);
            //        T1Query.setVisible(true);
            //    }
            //},
            {
                text: '新增',
                id: 'btnAdd2',
                name: 'btnAdd2',
                handler: function () {
                    T1Form.setVisible(false);
                    T2Form.setVisible(true);
                    T1Set = '/api/AB0021/DetailCreate';
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
                    //T1Query.setVisible(false);
                    T1Set = '/api/AB0021/DetailUpdate';
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
                        //let name = '';
                        let docno = '';
                        let seq = '';
                        //selection.map(item => {
                        //    //name += '「' + item.get('SEQ') + '」<br>';
                        //    docno += item.get('DOCNO') + ',';
                        //    seq += item.get('SEQ') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            docno += item.get('DOCNO') + ',';
                            seq += item.get('SEQ') + ',';
                        })

                        Ext.MessageBox.confirm('刪除', '是否確定刪除項次?<br>', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0021/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:刪除成功');
                                            //T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();

                                            Ext.getCmp('btnUpdate2').setDisabled(true);
                                            Ext.getCmp('btnDel2').setDisabled(true);
                                            Ext.getCmp('btnAdd2').setDisabled(true);

                                            T2Cleanup();

                                            T2Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
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

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('填寫');

        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCD'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();

            f.findField("DOCNO2").setValue(T1LastRec.data["DOCNO"]);
            f.findField("MAT_CLASS2").setValue(T1LastRec.data["MAT_CLASS"]);

            if (T1LastRec != null) {
                var tmpArray2 = T1LastRec.data["FRWH"].split(' ');
                f.findField('FRWH1').setValue(tmpArray2[0]);
            }
            
        }
        else {
            u = f.findField('MMCODE');

            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("MAT_CLASS2").setValue(T1LastRec.data["MAT_CLASS"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
        }
        f.findField('x').setValue(x);
        f.findField('MMCODE').setReadOnly(false);
        f.findField('APPQTY').setReadOnly(false);
        f.findField('APLYITEM_NOTE').setReadOnly(false);
        T2Form.down('#btnMmcode').setVisible(true);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        u.focus();
    }

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
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
                width: 100
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100
            },
            {
                text: "中文品名",
                dataIndex: 'MMNAME_C',
                width: 200
            },
            {
                text: "英文品名",
                dataIndex: 'MMNAME_E',
                width: 200
            },
            {
                text: "繳回數量",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "出庫庫存",
                dataIndex: 'INV_QTY_FR',
                width: 70,
                align: 'right',
            },
            {
                text: "入庫庫存",
                dataIndex: 'INV_QTY_TO',
                width: 70,
                align: 'right',
            },
            {
                text: "進貨單價",
                dataIndex: 'M_CONTPRICE',
                width: 70,
                align: 'right',
            },
            //{
            //    text: "核撥數量",
            //    dataIndex: 'APVQTY',
            //    width: 70,
            //    editor: {
            //        xtype: 'textfield',
            //        regexText: '只能輸入數字',
            //        regex: /^[0-9]+$/, // 用正規表示式限制可輸入內容
            //    }
            //},
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
                        var tmpArray = T1LastRec.data["FLOWID"].split(' ');
                        if (isGrade) {  // 庫房人員
                            //if (tmpArray[0] === '1' || tmpArray[0] === '3') {   // 1-申請中, 3-已繳回
                            //    Ext.getCmp('btnReject').setDisabled(true);
                            //    Ext.getCmp('btnSubmit2').setDisabled(true);
                            //}
                            //else {
                            //    Ext.getCmp('btnReject').setDisabled(false);
                            //    Ext.getCmp('btnSubmit2').setDisabled(false);
                            //}

                            Ext.getCmp('btnReject').setDisabled(true);
                            Ext.getCmp('btnSubmit2').setDisabled(true);

                        }
                        else {  // 申請人員
                            if (tmpArray[0] === '2' || tmpArray[0] === '3') {   // 2-繳回中, 3-已繳回
                                Ext.getCmp('btnUpdate').setDisabled(true);
                                Ext.getCmp('btnUpdate2').setDisabled(true);
                                Ext.getCmp('btnDel').setDisabled(true);
                                Ext.getCmp('btnDel2').setDisabled(true);
                                Ext.getCmp('btnAdd2').setDisabled(true);
                                Ext.getCmp('btnSubmit').setDisabled(true);
                            }
                            else {
                                Ext.getCmp('btnUpdate').setDisabled(true);
                                Ext.getCmp('btnUpdate2').setDisabled(false);
                                Ext.getCmp('btnDel').setDisabled(true);
                                Ext.getCmp('btnDel2').setDisabled(false);
                                Ext.getCmp('btnAdd2').setDisabled(false);
                                Ext.getCmp('btnSubmit').setDisabled(true);
                            }
                        }

                        if (T1Set === '')
                            setFormT2a();
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                setFormT2a();
            }
        }
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        
        if (T1LastRec != null && T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("MAT_CLASS2").setValue(T1LastRec.data["MAT_CLASS"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('M_CONTPRICE').setValue(T2LastRec.data["M_CONTPRICE"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);
            f.findField("APLYITEM_NOTE").setValue(T2LastRec.data["APLYITEM_NOTE"]);
            
            var tmpArray2 = T1LastRec.data["FRWH"].split(' ');
            f.findField('FRWH1').setValue(tmpArray2[0]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
            f.findField("APLYITEM_NOTE").setReadOnly(true);
            T2Form.down('#btnMmcode').setVisible(false);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);
        }

    }

    function T1Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        f.findField('MAT_CLASS').setReadOnly(true);
        f.findField('APPLY_NOTE').setReadOnly(true);
        f.findField('FRWH').setReadOnly(true);
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        T1Query.getForm().findField('P2').setValue(arrP2);
        setT1Matclassdefault();
        //setFormT1a();
    }

    function setMmcode(args) {
        var f = T2Form.getForm();
        if (args.MMCODE !== '') {
            f.findField("MMCODE").setValue(args.MMCODE);
            f.findField("MMNAME_C").setValue(args.MMNAME_C);
            f.findField("MMNAME_E").setValue(args.MMNAME_E);
            f.findField("M_CONTPRICE").setValue(args.M_CONTPRICE);
            f.findField("BASE_UNIT").setValue(args.BASE_UNIT);
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
        queryUrl: '/api/AB0021/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T2Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                var tmpArray = f.findField("MAT_CLASS2").getValue().split(' ');
                var tmpArray2 = T1LastRec.data["FRWH"].split(' ');
                return {
                    MAT_CLASS: tmpArray[0],
                    WH_NO: tmpArray2[0]
                };
            }
        },
        listeners: {
            select: function (c, r, i, e) {
                //選取下拉項目時，顯示回傳值
                //alert(r.get('MAT_CLASS'));
                var f = T2Form.getForm();
                if (r.get('MMCODE') !== '') {
                    f.findField("MMCODE").setValue(r.get('MMCODE'));
                    f.findField("MMNAME_C").setValue(r.get('MMNAME_C'));
                    f.findField("MMNAME_E").setValue(r.get('MMNAME_E'));
                    f.findField("BASE_UNIT").setValue(r.get('BASE_UNIT'));
                }
            }
        }
    });

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
            width: 300,
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
                xtype: 'hidden'
            },
            {
                fieldLabel: '出庫庫房',
                name: 'FRWH1',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '物料分類',
                name: 'MAT_CLASS2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            //{
            //    xtype: 'textfield',
            //    fieldLabel: '院內碼',
            //    name: 'MMCODE',
            //    editable: false,
            //    submitValue: true,
            //    allowBlank: false,
            //    fieldCls: 'required',
            //    listeners: {
            //        render: function () {
            //            this.getEl().on('mousedown', function (e, t, eOpts) {
            //                var f = T2Form.getForm();
            //                if (!f.findField("MMCODE").readOnly) {
            //                    tmpArray = f.findField("MAT_CLASS2").getValue().split(' ');
            //                    popMmcodeForm(viewport, '/api/AB0021/GetMmcode', { MMCODE: f.findField("MMCODE").getValue(), MAT_CLASS: tmpArray[0] }, setMmcode);
            //                }
            //            });
            //        }
            //    }
            //},
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
                                var tmpArray = f.findField("MAT_CLASS2").getValue().split(' ');
                                var tmpArray2 = T1LastRec.data["FRWH"].split(' ');
                                popMmcodeForm(viewport, '/api/AB0021/GetMmcode', { MMCODE: f.findField("MMCODE").getValue(), MAT_CLASS: tmpArray[0], WH_NO: tmpArray2[0] }, setMmcode);
                            }
                        }
                    },

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
            },
            {
                xtype: 'displayfield',
                fieldLabel: '進貨單價',
                name: 'M_CONTPRICE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
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
                name: 'APPQTY',
                enforceMaxLength: true,
                maxLength: 8,
                submitValue: true,
                allowBlank: false,
                fieldCls: 'required',
            },
            {
                xtype: 'textfield',
                fieldLabel: '備註',
                name: 'APLYITEM_NOTE',
                enforceMaxLength: true,
                maxLength: 8,
                submitValue: true
            },
        ],
        buttons: [
            {
                itemId: 'submit', text: '儲存', hidden: true,
                handler: function () {
                    if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                        //if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == ''
                        //    && this.up('form').getForm().findField('AGEN_NAMEE').getValue() == '')
                        //    Ext.Msg.alert('提醒', '廠商中文名稱或廠商英文名稱至少需輸入一種');
                        //else {
                        //    var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        //    Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                        //        if (btn === 'yes') {
                        //            T1Submit();
                        //        }
                        //    });
                        //}

                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T2Submit();
                                //T1Set = '';
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

    function T2Cleanup() {
        T1Set = '';
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T2Form.getForm();
        f.reset();
        //f.getFields().each(function (fc) {
        //    if (fc.xtype === "displayfield" || fc.xtype === "textfield") {
        //        fc.setReadOnly(true);
        //    } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
        //        fc.setReadOnly(true);
        //    }
        //});
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
        //setFormT1a();
    }

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
                            //var v = action.result.etts[0];
                            //T1Query.getForm().findField('P0').setValue(v.DN);
                            T2Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
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
    UserInfoLoad();
    DocnoLoad();

    function setT1Matclassdefault() {
        Ext.Ajax.request({
            url: '/api/AB0021/GetTask',
            method: reqVal_p,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    if (data.msg == "3") {
                        arrP3 = ["03", "04", "05", "06", "07"];
                    }
                    else if (data.msg == "2") {
                        arrP3 = ["02"];
                    }
                    else {
                        arrP3 = [""];
                    }
                    T1Query.getForm().findField('P3').setValue(arrP3);
                }
            },
            failure: function (response) {
                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            }
        });
    }
    
    setT1Matclassdefault();
    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnExp').setDisabled(true);
    Ext.getCmp('btnSubmit2').setDisabled(true);
    Ext.getCmp('btnReject').setDisabled(true);

    Ext.getCmp('btnAdd2').setDisabled(true);
    Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);
});