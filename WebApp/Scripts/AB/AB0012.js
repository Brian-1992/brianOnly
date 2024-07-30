Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);


Ext.onReady(function () {
    Ext.Ajax.setTimeout(600000);
    var T1Set = '';
    var T1LastRec, T2LastRec;
    var T1Name = '公藥申請';
    var userId, userName, userInid, userInidName, userTaskId;

    var viewModel = Ext.create('WEBAPP.store.AB.AB0012VM');
    var T1Store = viewModel.getStore('ME_DOCM');
    function T1Load() {
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p2", T1Query.getForm().findField('P2').getValue());
        T1Store.getProxy().setExtraParam("p3", session['Inid']);
        //T1Store.getProxy().setExtraParam("p3", T1Query.getForm().findField('P3').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        T1Store.getProxy().setExtraParam("p5", T1Query.getForm().findField('P5').getValue());
        T1Store.getProxy().setExtraParam("d0", T1Query.getForm().findField('D0').getValue());
        T1Store.getProxy().setExtraParam("d1", T1Query.getForm().findField('D1').getValue());
        T1Store.getProxy().setExtraParam("FLOWID", T1Query.getForm().findField('FLOWID').getValue());

        T1Store.getProxy().setExtraParam("APPDEPT", userInid);
        T1Store.getProxy().setExtraParam("DOCTYPE", 'MS');
        //T1Store.getProxy().setExtraParam("FLOWID", '0601');
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
            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["DOCNO"]);
            var tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
            T2Store.getProxy().setExtraParam("TOWH", tmpArray[0]);
            tmpArray = T1LastRec.data["FRWH_NAME"].split(" ");
            T2Store.getProxy().setExtraParam("FRWH", tmpArray[0]);

            T2Store.load({
                params: {
                    start: 0
                },
                callback: function () {
                    //if (T1LastRec.get('DOCNO') != '*') {
                        if (T2Store.getCount() > 0)
                            T2Grid.editingPlugin.startEdit(0, 6);
                    //}
                }
            });
        }
    }

    var st_frwhcombo = Ext.create('Ext.data.Store', {
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/AB0012/GetFrwhComboQ',
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
            url: '/api/AB0012/GetTowhComboQ',
            reader: {
                type: 'json',
                rootProperty: 'etts'
            }
        },
        autoLoad: true
    });
    var TowhStore = viewModel.getStore('TOWH');
    function TowhLoad() {
        //TowhStore.getProxy().setExtraParam("INID", session['Inid']);
        //TowhStore.load({
        //    params: {
        //        start: 0
        //    }
        //});
        TowhStore.on('load', function () {
            var f = T1Form.getForm();
            f.findField("TOWH").setValue(TowhStore.getAt(0).get('VALUE'));

            //Ext.getCmp('FRWH').setValue('');
            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: TowhStore.getAt(0).get('VALUE')
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    //FrwhLoad(data);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
                //jsonData: data
            });
        });
    }

    //var FrwhStore = viewModel.getStore('FRWH');
    //function FrwhLoad(wh_grade) {
    //    FrwhStore.getProxy().setExtraParam("INID", session['Inid']);
    //    FrwhStore.getProxy().setExtraParam("WH_GRADE", wh_grade);

    //    FrwhStore.on('load', function () {
    //        var f = T1Form.getForm();
    //        f.findField("FRWH").setValue(FrwhStore.getAt(0).get('WH_NO'));
    //    });

    //    FrwhStore.load({
    //        params: {
    //            start: 0
    //        }
    //    });
    //}

    var MatClassStore = viewModel.getStore('MAT_CLASS');
    function MatClassLoad() {
        MatClassStore.load({
            params: {
                start: 0
            }
        });
    }

    var UserInfoStore = viewModel.getStore('USER_INFO');
    function UserInfoLoad() {
        UserInfoStore.load(function (records, operation, success) {
            if (success) {
                var r = records[0];
                userId = r.get('TUSER');
                userName = r.get('UNA');
                userInid = r.get('INID');
                userInidName = r.get('INID_NAME');
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
                xtype: 'textfield',
                fieldLabel: '申請單號',
                name: 'P1',
                id: 'P1',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'datefield',
                fieldLabel: '申請日期',
                name: 'D0',
                id: 'D0',
                regex: /\d{7,7}/,
                labelWidth: 90,
                enforceMaxLength: true, // 限制可輸入最大長度
                maxLength: 7, // 可輸入最大長度為7
                padding: '0 4 0 4',
                value: getToday()
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
                xtype: 'textfield',
                fieldLabel: '申請人',
                name: 'P2',
                id: 'P2',
                enforceMaxLength: true,
                maxLength: 21,
                width: 300,
                labelWidth: 90,
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'combo',
                fieldLabel: '領用庫房',
                name: 'P4',
                id: 'P4',
                store: st_towhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                padding: '0 4 0 4'
            },
            {
                //xtype: 'textfield',
                xtype: 'combo',
                fieldLabel: '核撥庫房',
                name: 'P5',
                id: 'P5',
                store: st_frwhcombo,
                queryMode: 'local',
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{COMBITEM}&nbsp;</div></tpl>',
                forceSelection: true,
                queryMode: 'local',
                triggerAction: 'all',
                padding: '0 4 0 4'
            },
            {
                xtype: 'checkboxgroup',
                fieldLabel: '狀態',
                name: 'FLOWID',
                columns: 1,
                vertical: true,
                items: [
                    { boxLabel: '手動申請', name: 'rb', inputValue: '0601', checked: true },
                    { boxLabel: '自動申請', name: 'rb', inputValue: '0600', checked: true },
                    { boxLabel: '核撥中', name: 'rb', inputValue: '0602', checked: true },
                    { boxLabel: '點收中', name: 'rb', inputValue: '0603', checked: true },
                    { boxLabel: '沖帳中', name: 'rb', inputValue: '0604', checked: true },
                    { boxLabel: '已結案', name: 'rb', inputValue: '0699' }
                ]
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
                    //Ext.getCmp('btnUpdate2').setDisabled(true);
                    Ext.getCmp('btnDel').setDisabled(true);
                    Ext.getCmp('btnDel2').setDisabled(true);
                    //Ext.getCmp('btnAdd2').setDisabled(true);
                    Ext.getCmp('btnMultiMmcode').setDisabled(true);
                    Ext.getCmp('btnSubmit').setDisabled(true);
                    Ext.getCmp('btnCopy').setDisabled(true);
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
                    T1Set = '/api/AB0012/MasterCreate';
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
                    T1Set = '/api/AB0012/MasterUpdate';
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

                        Ext.MessageBox.confirm('刪除', '是否確定刪除申請單號?<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0012/MasterDelete',
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
                text: '複製',
                id: 'btnCopy',
                name: 'btnCopy',
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

                        Ext.MessageBox.confirm('訊息', '確認是否複製?<br>單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0012/Copy',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno
                                    },
                                    //async: true,
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            msglabel('訊息區:複製成功');
                                            T2Store.removeAll();
                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            Ext.getCmp('btnUpdate').setDisabled(true);
                                            Ext.getCmp('btnDel').setDisabled(true);
                                            Ext.getCmp('btnSubmit').setDisabled(true);
                                            Ext.getCmp('btnCopy').setDisabled(true);
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
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                text: '提出申請',
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

                        Ext.MessageBox.confirm('訊息', '確認是否申請?<br>單號如下<br>' + name, function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/AB0012/UpdateStatusBySP',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        //FLOWID: '0602'
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
                                            Ext.getCmp('btnCopy').setDisabled(true);
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
                    else {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                }
            },
            {
                text: '匯入管制藥品',
                id: 'btnImport',
                name: 'btnImport',
                handler: function () {
                    if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }

                    if (T2Store.getCount() > 0) {
                        Ext.MessageBox.alert('錯誤', '此單號已存在院內碼，不得再匯入管制藥品');
                        return;
                    }

                    Ext.MessageBox.confirm('訊息', '確認是否匯入管制藥品?' + name, function (btn, text) {
                        if (btn === 'yes') {
                            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
                            myMask.show();
                            Ext.Ajax.request({
                                //url: '/api/AB0012/MMRep_AB0012',
                                url: '/api/AB0012/Winctl_AB0012',
                                method: reqVal_p,
                                params: {
                                    DOCNO: T1LastRec.data["DOCNO"],
                                    WH_NO: T1LastRec.data["TOWH"]
                                },
                                //async: true,
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {
                                        msglabel('匯入成功');
                                        T2Store.removeAll();
                                        T2Load();
                                        T1Grid.getSelectionModel().deselectAll();
                                        T1Load();
                                        Ext.getCmp('btnUpdate').setDisabled(true);
                                        Ext.getCmp('btnDel').setDisabled(true);
                                        Ext.getCmp('btnSubmit').setDisabled(true);
                                        Ext.getCmp('btnCopy').setDisabled(true);
                                    }
                                    else {
                                        Ext.MessageBox.alert('錯誤', data.msg);
                                        msglabel('訊息區:' + data.msg);
                                    }
                                    myMask.hide();
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                    myMask.hide();
                                }
                            });
                        }
                    });
                }
            },
            {
                text: '麻醉及管制藥品登記表',
                id: 'btnExportApply',
                name: 'btnExportApply',
                handler: function () {
                    if (T1LastRec == null || T1LastRec.data["DOCNO"] == '') {
                        Ext.MessageBox.alert('錯誤', '尚未選取單據號碼');
                        return;
                    }
                    else if (T2Store.getCount() == 0)
                    {
                        Ext.MessageBox.alert('訊息', '此單據尚未加入任何品項');
                        return;
                    }
                    reportUrl = '/Report/A/AB0012a.aspx';
                    showReport();
                }
            }
        ]
    });

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
            //T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("TOWH");
            u.setReadOnly(false);
            u.clearInvalid();
            f.findField('TOWH').setReadOnly(false);
            //f.findField('FRWH').setReadOnly(false);

            f.findField("DOCNO").setValue('系統自編');
            f.findField("APPID").setValue(userId + ' ' + userName);
            f.findField("INID_NAME").setValue(userInid + ' ' + userInidName);
            f.findField("APPTIME").setValue(getChtToday());
        }
        else {
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("APPID").setValue(T1LastRec.data["APP_NAME"]);
            f.findField("INID_NAME").setValue(T1LastRec.data["APPDEPT_NAME"]);
            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

            u = f.findField('TOWH');
            u.setReadOnly(false);
            //f.findField('FRWH').setReadOnly(false);

            var tmpArray = T1LastRec.data["TOWH_NAME"].split(" ");
            f.findField("TOWH").setValue(tmpArray[0]);

            // 因為領用部門和核撥庫房有連動, 所以都要重新取得
            //Ext.Ajax.request({
            //    url: '/api/AB0010/GetGrade',
            //    method: reqVal_p,
            //    params: {
            //        WH_NO: tmpArray[0]
            //    },
            //    success: function (response) {
            //        var data = Ext.decode(response.responseText);
            //        FrwhLoad(data);

            //        tmpArray = T1LastRec.data["FRWH_NAME"].split(" ");
            //        f.findField('FRWH').setValue(tmpArray[0]);
            //    },
            //    failure: function (response) {
            //        Ext.MessageBox.alert('錯誤', '發生例外錯誤');
            //    },
            //});

        }
        f.findField('x').setValue(x);
        //f.findField('MAT_CLASS').setReadOnly(false);
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
                fieldLabel: '申請單號',
                name: 'DOCNO',
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
                xtype: 'displayfield',
                fieldLabel: '申請人員',
                name: 'APPID',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單位',
                name: 'INID_NAME',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'combo',
                fieldLabel: '領用庫房',
                name: 'TOWH',
                id: 'TOWH',
                store: TowhStore,
                displayField: 'COMBITEM',
                valueField: 'VALUE',
                readOnly: true,
                editable: false,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                listeners: {
                    //change: function (e, newValue, oldValue, eOpts) {
                    select: function (combo, record, eOpts) {
                        Ext.getCmp('FRWH').setValue('');
                        Ext.Ajax.request({
                            url: '/api/AB0010/GetGrade',
                            method: reqVal_p,
                            params: {
                                //WH_NO: newValue
                                WH_NO: record.get("VALUE")
                            },
                            success: function (response) {
                                var data = Ext.decode(response.responseText);
                                //FrwhLoad(data);
                            },
                            failure: function (response) {
                                Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                            }
                            //jsonData: data
                        });
                    }
                }
            }
            //{
            //    xtype: 'combo',
            //    fieldLabel: '核撥庫房',
            //    name: 'FRWH',
            //    id: 'FRWH',
            //    store: FrwhStore,
            //    displayField: 'WH_NAME',
            //    valueField: 'WH_NO',
            //    readOnly: true,
            //    editable: false,
            //    allowBlank: false, // 欄位為必填
            //    //anyMatch: true,
            //    //typeAhead: true,
            //    //forceSelection: true,
            //    //queryMode: 'local',
            //    //triggerAction: 'all',
            //    fieldCls: 'required'
            //}
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

    function getTodayDate() {
        var fullDate = new Date();
        var yyy = fullDate.getFullYear() - 1911;
        var MM = (fullDate.getMonth() + 1) >= 10 ? (fullDate.getMonth() + 1) : ("0" + (fullDate.getMonth() + 1));
        var dd = fullDate.getDate() < 10 ? ("0" + fullDate.getDate()) : fullDate.getDate();
        var today = yyy + MM + dd;
        return today;
    }

    function getToday() {
        var today = new Date();
        today.setDate(today.getDate());
        return today
    }
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
                            T2Store.removeAll();
                            T1Query.getForm().findField('D0').setValue(getTodayDate());
                            T1Query.getForm().findField('D1').setValue(getTodayDate());
                            //var v = action.result.etts[0];
                            //T1Query.getForm().findField('P1').setValue(v.DOCNO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            T1Load();
                            msglabel('訊息區:資料更新成功');
                            T1Set = '';
                            break;
                        case "R":
                            T1Load();
                            msglabel('訊息區:資料退回成功');
                            T1Set = '';
                            break;
                    }

                    T1Cleanup();
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
        //selModel: {
        //    checkOnly: false,
        //    injectCheckbox: 'first',
        //    mode: 'MULTI'
        //},
        //selType: 'checkboxmodel',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 100
            },
            {
                text: "狀態",
                dataIndex: 'FLOWID',
                width: 120
            },
            {
                text: "申請人員",
                dataIndex: 'APPID',
                width: 150
            },
            {
                xtype: 'hidden',
                text: "申請人員",
                dataIndex: 'APP_NAME',
                width: 150
            },
            {
                text: "申請部門",
                dataIndex: 'APPDEPT_NAME',
                width: 200
            },
            {
                text: "申請日期",
                dataIndex: 'APPTIME',
                width: 120
            },
            {
                text: "領用庫房",
                dataIndex: 'TOWH_NAME',
                width: 120
            },
            {
                text: "核撥庫房",
                dataIndex: 'FRWH_NAME',
                width: 120
            }, 
            {
                text: "單據退回備註",
                dataIndex: 'RETURN_NOTE',
                width: 200
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

                        Ext.getCmp('btnUpdate').setDisabled(false);
                        Ext.getCmp('btnDel').setDisabled(false);

                        //Ext.getCmp('btnUpdate2').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(true);
                        //Ext.getCmp('btnAdd2').setDisabled(false);
                        Ext.getCmp('btnMultiMmcode').setDisabled(false);
                        Ext.getCmp('btnSubmit').setDisabled(false);
                        Ext.getCmp('btnCopy').setDisabled(false);
                        Ext.getCmp('btnImport').setDisabled(false); 
                        Ext.getCmp('btnExportApply').setDisabled(false);
                        Ext.getCmp('export').setDisabled(false);


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

                if (T1LastRec == null) {
                    //Ext.getCmp('btnAdd2').setDisabled(true);
                    Ext.getCmp('btnMultiMmcode').setDisabled(true);
                } 
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
            var tmpArray = T1LastRec.data["TOWH_NAME"].split(' ');
            f.findField("TOWH").setValue(tmpArray[0]);

            // 因為領用部門和核撥庫房有連動, 所以都要重新取得
            Ext.Ajax.request({
                url: '/api/AB0010/GetGrade',
                method: reqVal_p,
                params: {
                    WH_NO: tmpArray[0]
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    //FrwhLoad(data);

                    tmpArray = T1LastRec.data["FRWH_NAME"].split(" ");

                    // 切換T1Grid時, 如果值一樣,還做setValue()的話,欄位會被清空,所以值不同再做setValue()
                    //if (f.findField('FRWH').getValue() != tmpArray[0])
                    //    f.findField('FRWH').setValue(tmpArray[0]);
                },
                failure: function (response) {
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                },
            });

            f.findField("APPTIME").setValue(T1LastRec.data["APPTIME"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);

            tmpArray = T1LastRec.data["APP_NAME"].split(' ');
            tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
            //if (session['UserId'] !== tmpArray[0] || tmpArray1[0] !== '0601') { 20191211
            if (tmpArray1[0] !== '0601') {
                Ext.getCmp("btnUpdate").setDisabled(true);
                Ext.getCmp("btnDel").setDisabled(true);
                Ext.getCmp("btnSubmit").setDisabled(true);
                //Ext.getCmp("btnAdd2").setDisabled(true);
                Ext.getCmp('btnMultiMmcode').setDisabled(true);
                Ext.getCmp('btnImport').setDisabled(true); 
                Ext.getCmp('export').setDisabled(false);
            }
            else {
                Ext.getCmp("btnUpdate").setDisabled(false);
                Ext.getCmp("btnDel").setDisabled(false);
                Ext.getCmp("btnSubmit").setDisabled(false);
                //Ext.getCmp("btnAdd2").setDisabled(false);
                Ext.getCmp('btnMultiMmcode').setDisabled(false);
                Ext.getCmp('btnImport').setDisabled(false);
                Ext.getCmp('export').setDisabled(true);
            }
            if (tmpArray1[0] == '0601' || tmpArray1[0] == '0602')
                Ext.getCmp('btnExportApply').setDisabled(false);
            else
                Ext.getCmp('btnExportApply').setDisabled(true);

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
            //{
            //    text: '新增',
            //    id: 'btnAdd2',
            //    name: 'btnAdd2',
            //    handler: function () {
            //        T1Form.setVisible(false);
            //        T2Form.setVisible(true);
            //        T1Set = '/api/AB0012/DetailCreate';
            //        setFormT2('I', '新增');
            //    }
            //},
            //{
            //    text: '修改',
            //    id: 'btnUpdate2',
            //    name: 'btnUpdate2',
            //    handler: function () {
            //        T1Form.setVisible(false);
            //        T2Form.setVisible(true);
            //        //T1Query.setVisible(false);
            //        T1Set = '/api/AB0012/DetailUpdate';
            //        setFormT2('U', '修改');
            //    }
            //},
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
                                    url: '/api/AB0012/DetailDelete',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO: docno,
                                        SEQ: seq
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除項次<br>' + name + '成功');
                                            msglabel('訊息區:刪除成功');
                                            //T2Store.removeAll();
                                            T2Grid.getSelectionModel().deselectAll();
                                            T2Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
                                        } else {
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
                text: '選取公藥',
                id: 'btnMultiMmcode',
                handler: function () {
                    if (T1LastRec != null) {
                        var tmpArray = T1LastRec.data["FRWH_NAME"].split(' ');
                        var frwh = tmpArray[0];
                        tmpArray = T1LastRec.data["TOWH_NAME"].split(' ')
                        var towh = tmpArray[0];

                        popMmcodeForm_multi(viewport, '/api/AB0012/GetMultiMmcode', { MMCODE: '', WH_NO: '', FRWH: frwh, TOWH: towh }, addMmcode);
                    }

                }
            },
            //{
            //    itemId: 'export',
            //    text: '匯出',
            //    id: 'export',
            //    name: 'export',
            //    handler: function () {

            //        var p0 = T1LastRec.get('DOCNO');
            //        var tmpArray = T1LastRec.data["FRWH_NAME"].split(' ');
            //        var p1 = tmpArray[0];

            //        var tmpArray = T1LastRec.data["TOWH_NAME"].split(' ');
            //        var p2 = tmpArray[0];

            //        var p = new Array();

            //        p.push({ name: 'P0', value: p0 });
            //        p.push({ name: 'P1', value: p1 });
            //        p.push({ name: 'P2', value: p2 });
            //        PostForm('../../../api/AB0012/Excel', p);
            //    }
            //},
            {
                xtype: 'button',
                text: '列印',
                id: 'export',
                name: 'export',
                handler: function () {
                    reportUrl = '/Report/A/AB0012.aspx';
                    showReport();
                }
            }
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
                html: '<iframe src="' + reportUrl + '?'
                + 'docno=' + T1LastRec.data["DOCNO"]
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

    function addMmcode(args) {
        if (args.MMCODE !== '') {
            //alert(T1LastRec.data["DOCNO"]);
            var myMask = new Ext.LoadMask(viewport, { msg: '處理中...' });
            myMask.show();
            Ext.Ajax.request({
                url: '/api/AB0012/DetailCreate_multi',
                method: reqVal_p,
                params: {
                    DOCNO: T1LastRec.data["DOCNO"],
                    MMCODE: args.MMCODE
                },
                success: function (response) {
                    var data = Ext.decode(response.responseText);
                    if (data.success) {
                        msglabel('新增成功');
                        //T2Store.removeAll();
                        T2Grid.getSelectionModel().deselectAll();
                        T2Load();
                        //Ext.getCmp('btnSubmit').setDisabled(true);
                    } else {
                        Ext.MessageBox.alert('錯誤', data.msg);
                        msglabel('訊息區:' + data.msg);
                    }
                    myMask.hide();
                },
                failure: function (response) {
                    myMask.hide();
                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                }
            });
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
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCD'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();

            f.findField("DOCNO2").setValue(T1LastRec.data["DOCNO"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
        }
        else {
            u = f.findField('MMCODE');

            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
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
                text: "申請單號",
                dataIndex: 'DOCNO',
                width: 100,
                locked: true
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100,
                locked: true,
                renderer: function (val, meta, record) {
                    if (val != null) 
                        return '<a href="http://eserver15.ndmctsgh.edu.tw/med/web/MedDetail.aspx?ORDERCODE=' + val + '\" target="_blank" rel="noopener noreferrer">' + val + '</a>';
                }
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
                text: "<span style='color:red'>申請數量</span>",
                dataIndex: 'APPQTY',
                width: 70,
                align: 'right',
                editor: {
                    fieldCls: 'required',
                    selectOnFocus: true,
                    allowBlank: false,
                    minValue: 0,
                    listeners: {
                        specialkey: function (field, e) {
                            //if (e.getKey() === e.LEFT) {
                            //    var editPlugin = this.up().editingPlugin;
                            //    editPlugin.startEdit(editPlugin.context.rowIdx, 3);
                            //}

                            if (e.getKey() === e.RIGHT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 7);
                            }
                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 6);
                            }

                            if (e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 6);
                            }
                        }
                    }
                }
            },
            {
                text: "<span style='color:red'>備註</span>",
                dataIndex: 'APLYITEM_NOTE',
                width: 70,
                align: 'left',
                editor: {
                    fieldStyle: 'text-align:left;',
                    listeners: {
                        specialkey: function (field, e) {
                            if (e.getKey() === e.LEFT) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.startEdit(editPlugin.context.rowIdx, 6);
                            }

                            if (e.getKey() === e.UP) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx - 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx - 1, 7);
                            }

                            if (e.getKey() === e.DOWN) {
                                var editPlugin = this.up().editingPlugin;
                                editPlugin.completeEdit();
                                var sm = T2Grid.getSelectionModel();
                                sm.deselectAll();
                                sm.select(editPlugin.context.rowIdx + 1);
                                editPlugin.startEdit(editPlugin.context.rowIdx + 1, 7);
                            }
                        }
                    }
                }
            },
            {
                text: "單價",
                dataIndex: 'DISC_CPRICE',
                width: 70,
            },
            {
                text: "計量單位",
                dataIndex: 'BASE_UNIT',
                width: 70,
            },
            {
                text: "核撥數量",
                dataIndex: 'APVQTY',
                align: 'right',
                width: 70,
            },
            {
                text: "核撥日期",
                dataIndex: 'APVTIME',
                width: 70,
            },
            {
                text: "未核撥數量",
                dataIndex: 'APP_QTY_NOT_APPROVED',
                align: 'right',
                width: 90,
            },
            {
                text: "點收數量",
                dataIndex: 'ACKQTY',
                align: 'right',
                width: 70,
            },
            {
                text: "點收日期",
                dataIndex: 'ACKTIME',
                width: 70,
            },
            {
                text: "上級庫庫存量",
                dataIndex: 'S_INV_QTY',
                align: 'right',
                width: 100,
            },
            {
                text: "庫存量",
                dataIndex: 'INV_QTY',
                align: 'right',
                width: 70,
            },
            {
                text: "安全量",
                dataIndex: 'SAFE_QTY',
                align: 'right',
                width: 70,
            },
            {
                text: "基準量",
                dataIndex: 'OPER_QTY',
                align: 'right',
                width: 70,
            },
            {
                text: "包裝劑量",
                dataIndex: 'PACK_QTY',
                align: 'right',
                width: 70,
            },
            {
                text: "包裝單位",
                dataIndex: 'PACK_UNIT',
                width: 70,
            },
            {
                text: "藥品停用碼",
                dataIndex: 'E_ORDERDCFLAG',
                width: 90,
            },
            {
                text: "出庫庫房",
                dataIndex: 'FRWH_D',
                width: 70,
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
                    //if (T2Form.hidden === true) {
                    //    T1Form.setVisible(false);
                    //    T2Form.setVisible(true);
                    //}
                    Ext.getCmp('eastform').collapse();

                    // grid中所有click都會觸發, 所以要判斷真的有選取到一筆record才能執行
                    if (T2LastRec != null) {


                        Ext.getCmp('btnUpdate').setDisabled(true);
                        //Ext.getCmp('btnUpdate2').setDisabled(false);
                        Ext.getCmp('btnDel').setDisabled(true);
                        Ext.getCmp('btnDel2').setDisabled(false);
                        //Ext.getCmp('btnAdd2').setDisabled(false);
                        Ext.getCmp('btnMultiMmcode').setDisabled(false);
                        if (T1LastRec == null) {
                            Ext.getCmp('btnSubmit').setDisabled(true);
                            Ext.getCmp('btnCopy').setDisabled(true);
                            Ext.getCmp('btnImport').setDisabled(true);
                            Ext.getCmp('btnExportApply').setDisabled(true);
                        }


                        //if (T1Set === '')
                        //    setFormT2a();
                    }
                }
            },
            selectionchange: function (model, records) {
                T2Rec = records.length;
                T2LastRec = records[0];
                //setFormT2a();
            },
            beforeedit: function (plugin, context) {
                if (T1LastRec != null)
                {
                    tmpArray = T1LastRec.data["APP_NAME"].split(' ');
                    tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
                    //if (session['UserId'] !== tmpArray[0] || tmpArray1[0] !== '0601') 20191211
                    if (tmpArray1[0] !== '0601')
                        return false;
                    else
                        return true;
                }
            },
        },
        plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    validateedit: function (editor, context, eOpts) {
                    },
                    edit: function (editor, context, eOpts) {
                        var modifiedRec = context.store.getModifiedRecords();
                        var r = context.record;
                        var docno = r.get('DOCNO');
                        var mmcode = (context.field === 'MMCODE') ? context.value : r.get('MMCODE');
                        var appqty = (context.field === 'APPQTY') ? context.value : r.get('APPQTY');
                        var itemnote = (context.field === 'APLYITEM_NOTE') ? context.value : r.get('APLYITEM_NOTE');

                        Ext.each(modifiedRec, function (item) {
                            if (item.crudState === 'U') {
                                Ext.Ajax.request({
                                    url: '/api/AB0012/DetailUpdate',
                                    method: reqVal_p,
                                    params: {
                                        DOCNO2: T1LastRec.get('DOCNO'),
                                        SEQ: r.get('SEQ'),
                                        //FRWH2: T1LastRec.get('FRWH'),
                                        MMCODE: mmcode,
                                        APPQTY: appqty,
                                        APLYITEM_NOTE: itemnote
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            item.commit();
                                            msglabel('資料更新成功');

                                            editor.startEdit(context.rowIdx + 1, 6);
                                        }
                                        else {
                                            item.reject();
                                            Ext.Msg.alert('提醒', data.msg, function () {
                                                editor.startEdit(context.rowIdx, 6);
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
                    },
                    canceledit: function (editor, context, eOpts) {
                        //alert('onCancelEdit');
                    }
                }
            })
        ]
    });

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_NAME"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('M_CONTPRICE').setValue(T2LastRec.data["M_CONTPRICE"]);
            f.findField('BASE_UNIT').setValue(T2LastRec.data["BASE_UNIT"]);
            f.findField('APPQTY').setValue(T2LastRec.data["APPQTY"]);

            f.findField('MMCODE').setReadOnly(true);
            f.findField('APPQTY').setReadOnly(true);
            T2Form.down('#btnMmcode').setVisible(false);
            T2Form.down('#cancel').setVisible(false);
            T2Form.down('#submit').setVisible(false);

            tmpArray = T1LastRec.data["APP_NAME"].split(' ');
            tmpArray1 = T1LastRec.data["FLOWID"].split(' ');
            //if (session['UserId'] !== tmpArray[0] || tmpArray1[0] !== '0601') {
            if (tmpArray1[0] !== '0601') {
                //Ext.getCmp("btnUpdate2").setDisabled(true);
                Ext.getCmp("btnDel2").setDisabled(true);
                //Ext.getCmp("btnAdd2").setDisabled(true);
                Ext.getCmp('btnMultiMmcode').setDisabled(true);
            }
            else {
                //Ext.getCmp("btnUpdate2").setDisabled(false);
                Ext.getCmp("btnDel2").setDisabled(false);
                //Ext.getCmp("btnAdd2").setDisabled(false);
                Ext.getCmp('btnMultiMmcode').setDisabled(false);
            }
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
        f.findField('TOWH').setReadOnly(true);
        //f.findField('FRWH').setReadOnly(true);
        viewport.down('#form').setTitle('瀏覽');
        var currentTab = Tabs.getActiveTab();
        currentTab.setTitle('瀏覽');
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
        queryUrl: '/api/AB0012/GetMMCodeCombo',

        //查詢完會回傳的欄位
        extraFields: ['MAT_CLASS', 'BASE_UNIT'],

        //查詢時Controller固定會收到的參數
        getDefaultParams: function () {
            var f = T2Form.getForm();
            if (!f.findField("MMCODE").readOnly) {
                //tmpArray = f.findField("FRWH2").getValue().split(' ');
                tmpArray = T1LastRec.data["TOWH"].split(' ');
                return {
                    WH_NO: tmpArray[0]
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
                fieldLabel: '項次',
                name: 'SEQ',
                xtype: 'hidden'
            },
            {
                xtype: 'displayfield',
                fieldLabel: '申請單號',
                name: 'DOCNO2',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '核撥庫房',
                name: 'FRWH2',
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
            //                    tmpArray = f.findField("FRWH2").getValue().split(' ');
            //                    popMmcodeForm(viewport, { MMCODE: f.findField("MMCODE").getValue(), WH_NO: tmpArray[0], E_IFPUBLIC: '0', closeCallback: setMmcode }); // E_IFPUBLIC=0 為公藥
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
                                //tmpArray = f.findField("FRWH2").getValue().split(' ');
                                tmpArray = T1LastRec.data["TOWH"].split(' ');
                                popMmcodeForm(viewport, '/api/AB0012/GetMmcode', { MMCODE: f.findField("MMCODE").getValue(), WH_NO: tmpArray[0] }, setMmcode);
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
                fieldLabel: '申請數量',
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
                            //T1Load();
                            msglabel('訊息區:資料新增成功');
                            T1Set = '';
                            break;
                        case "U":
                            T2Load();
                            //T1Load();
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
            //items: [T1Form, T2Form]
            items: [T1Form]
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
    TowhLoad();
    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnDel').setDisabled(true);
    Ext.getCmp('btnSubmit').setDisabled(true);
    Ext.getCmp('btnCopy').setDisabled(true);
    Ext.getCmp('btnImport').setDisabled(true); 
    Ext.getCmp('btnExportApply').setDisabled(true);
    Ext.getCmp('export').setDisabled(true);

    //Ext.getCmp('btnAdd2').setDisabled(true);
    //Ext.getCmp('btnUpdate2').setDisabled(true);
    Ext.getCmp('btnDel2').setDisabled(true);
    Ext.getCmp('btnMultiMmcode').setDisabled(true);
});