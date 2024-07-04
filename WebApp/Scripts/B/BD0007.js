Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    var T1Set = '';
    var T1LastRec = null, T2LastRec = null; 
    var T1Name = '廠商訊息傳送';

    var userId = session['UserId'];
    var userName = session['UserName'];
    var userInid = session['Inid'];
    var userInidName = session['InidName'];

    var viewModel = Ext.create('WEBAPP.store.BD0007VM');
    //var viewModel = Ext.create('WEBAPP.store.AB.AB0022VM');
    var MATComboGet = '../../../api/BA0002/GetMATCombo';  
    
    var T1Store = viewModel.getStore('MM_PO_M');
    // 物品類別清單
    var MATQueryStore = Ext.create('Ext.data.Store', {
        fields: ['VALUE', 'TEXT']
    });
    function setComboData() {
        Ext.Ajax.request({
            url: MATComboGet,
            method: reqVal_g,
            success: function (response) {
                var data = Ext.decode(response.responseText);
                if (data.success) {
                    var wh_nos = data.etts;
                    if (wh_nos.length > 0) {
                        for (var i = 0; i < wh_nos.length; i++) {
                            MATQueryStore.add({ VALUE: wh_nos[i].VALUE, TEXT: wh_nos[i].TEXT });
                        }
                        T1Query.getForm().findField('P4').setValue(wh_nos[0].VALUE);
                    }
                }
            },
            failure: function (response, options) {
            }
        });
    }
    setComboData();  
    function T1Load() {
        T2Store.removeAll();
        T2LastRec = null;
        T1Store.getProxy().setExtraParam("p0", T1Query.getForm().findField('P0').getValue());
        T1Store.getProxy().setExtraParam("p1", T1Query.getForm().findField('P1').getValue());
        T1Store.getProxy().setExtraParam("p4", T1Query.getForm().findField('P4').getValue());
        if (Ext.getCmp('radio1').getValue() == true) {
            T1Store.getProxy().setExtraParam("p2",Ext.getCmp('radio1').inputValue);
        } else {
            T1Store.getProxy().setExtraParam("p2",Ext.getCmp('radio2').inputValue);
        }

        T1Tool.moveFirst();
    }

    var T2Store = viewModel.getStore('MM_PO_D');
    function T2Load() {


        if (T1LastRec != null && T1LastRec.data["PO_NO"] !== '') {

            T2Store.getProxy().setExtraParam("p0", T1LastRec.data["PO_NO"]);
            T2Tool.moveFirst();

        }
    }

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
            layout: 'hbox',
            items: [
                {
                    xtype: 'panel',
                    id: 'PanelP1',
                    border: false,
                    layout: 'hbox',
                    items: [
                        {
                            xtype: 'datefield',
                            fieldLabel: '訂單日期',
                            name: 'P0',
                            id: 'P0',
                            enforceMaxLength: true,
                            maxLength: 21,
                            labelWidth: 60,
                            width: 140,
                            padding: '0 4 0 4',
                            allowBlank:false,
                            fieldCls:'required',
                            value: getDefaultValue(false),
                            regexText: '請選擇日期',
                        },
                        {
                            xtype: 'datefield',
                            fieldLabel: '至',
                            labelSeparator: '',
                            name: 'P1',
                            id: 'P1',
                            labelWidth: mLabelWidth,
                            width: 88,
                            labelWidth: 8,
                            padding: '0 4 0 4',
                            allowBlank: false,
                            fieldCls: 'required',
                            value: getDefaultValue(true),
                            regexText: '請選擇日期',
                        }, {
                            xtype: 'fieldcontainer',
                            fieldLabel: '回覆情況',
                            defaultType: 'radiofield',
                            labelWidth: 70,
                            defaults: {
                                flex: 1
                            },
                            layout: 'hbox',
                            items: [
                                {
                                    boxLabel: '已回覆',
                                    name: 'replyType',
                                    inputValue: 'Y',
                                    id: 'radio1'
                                    
                                }, {
                                    boxLabel: '未回覆',
                                    name: 'replyType',
                                    inputValue: 'N',
                                    id: 'radio2',value: true
                                }
                            ]
                        }, {
                            xtype: 'combo',
                            fieldLabel: '物料類別',
                            name: 'P4',
                            enforceMaxLength: true,
                            labelWidth: 60,
                            width: 170,
                            padding: '0 4 0 4',
                            store: MATQueryStore,
                            displayField: 'TEXT',
                            valueField: 'VALUE',
                            queryMode: 'local',
                            anyMatch: true,
                            fieldCls: 'required',
                            tpl: '<tpl for="."><div class="x-boundlist-item" style="height:auto;">{TEXT}&nbsp;</div></tpl>'
                        }, {
                            xtype: 'button',
                            text: '查詢',
                            handler: function () {
                                msglabel('訊息區:');
                                if (T1Query.getForm().isValid()) {
                                    T1Load();
                                } else {
                                    Ext.Msg.alert('提醒', '<span style=\'color:red\'>訂單日期</span>為必填');
                                }

                                
                            }
                        },
                        {
                            xtype: 'button',
                            text: '清除',
                            handler: function () {
                                var f = this.up('form').getForm();
                                f.reset();
                                f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                            }
                        }
                    ]
                }
            ]
        }]
    });
    function getDefaultValue(isEndDate) {
        var yyyy = new Date().getFullYear()-1911;
        var m = new Date().getMonth() + 1;
        var d = 0;
        if (isEndDate) {
            d = new Date(yyyy, m, 0).getDate();
        } else {
            d = 1;
        }
        var mm = m > 10 ? m.toString() : "0" + m.toString();
        var dd = d > 10 ? d.toString() : "0" + d.toString();

        return yyyy.toString() + mm + dd;
        
    }

    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        border: false,
        displayInfo: true,
        plain: true,
        buttons: [
            {
                text: '修改',
                id: 'btnUpdate',
                name: 'btnUpdate',
                handler: function () {
                    setFormVisible(true, false);
                    T1Set = '/api/BD0007/MasterUpdate';
                    setFormT1('U', '修改');
                }
            },
            {
                text: '寄送MAIL',
                id: 'btnSendMail',
                name: 'btnSendMail',
                handler: function () {
                    var selection = T1Grid.getSelection();
                    if (selection.length) {
                        //let name = '';
                        let po_no = '';
                        //selection.map(item => {
                        //   // name += '「' + item.get('PO_NO') + '」<br>';
                        //    po_no += item.get('PO_NO') + ',';
                        //});
                        $.map(selection, function (item, key) {
                            po_no += item.get('PO_NO') + ',';
                        })

                        Ext.MessageBox.confirm('寄送MAIL', '要進行採購訂單MAIL發送作業?', function (btn, text) {
                            if (btn === 'yes') {
                                Ext.Ajax.request({
                                    url: '/api/BD0007/SendEmail',
                                    method: reqVal_p,
                                    params: {
                                        PO_NO: po_no
                                    },
                                    success: function (response) {
                                        var data = Ext.decode(response.responseText);
                                        if (data.success) {
                                            //Ext.MessageBox.alert('訊息', '刪除申請單號<br>' + name + '成功');
                                            msglabel('訊息區:資料更新成功');
                                            //T2Store.removeAll();

                                            T1Grid.getSelectionModel().deselectAll();
                                            T1Load();
                                            //Ext.getCmp('btnSubmit').setDisabled(true);
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
        ]
    });

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
                layout: 'fit',
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
                text: "訂單編號",
                dataIndex: 'PO_NO',
                width: 120
            },
            {
                text: "廠商",
                dataIndex: 'AGEN_NO',
                width: 190
            },
            {
                text: "訂單狀態",
                dataIndex: 'PO_STATUS',
                width: 80
            },
            {
                text: "回覆情況",
                dataIndex: 'ISBACK',
                width: 80
            },
            {
                text: "MAIL備註",
                dataIndex: 'MEMO',
                width: 230
            },
            {
                text: "MAIL特殊備註",
                dataIndex: 'SMEMO',
                width: 230
            },
            {
                text: "EMAIL",
                dataIndex: 'EMAIL',
                width: 230
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                

                Ext.getCmp('btnUpdate').setDisabled(false);
                Ext.getCmp('btnSendMail').setDisabled(false);
                
                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T1LastRec = record;
                setFormT1a();

                setFormVisible(true, false);

                //Tabs.setActiveTab('Form');

                T2Load();
            }
        }
    });

    var T2Tool = Ext.create('Ext.PagingToolbar', {
        store: T2Store,
        border: false,
        plain: true,
        displayInfo: true
    });

    var T2Grid = Ext.create('Ext.grid.Panel', {
        //title: '核撥明細',
        store: T2Store,
        plain: true,
        loadingText: '處理中...',
        loadMask: true,
        cls: 'T1',
        columns: [
            {
                xtype: 'rownumberer'
            },
            {
                text: "院內碼",
                dataIndex: 'MMCODE',
                width: 100,
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
                text: "廠牌",
                dataIndex: 'M_AGENLAB',
                width: 100,
                style: 'text-align:left'
            },
           
            {
                text: "單位",
                dataIndex: 'M_PURUN',
                width: 70,
                style: 'text-align:left'
            },
            {
                text: "單價",
                dataIndex: 'PO_PRICE',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "申請量",
                dataIndex: 'PO_QTY',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "單筆價",
                dataIndex: 'PO_AMT',
                width: 70,
                align: 'right',
                style: 'text-align:left'
            },
            {
                text: "備註",
                dataIndex: 'MEMO',
                width: 70,
                style: 'text-align:left'
            },
            {
                header: "",
                flex: 1
            }
        ],
        listeners: {
            itemclick: function (self, record, item, index, e, eOpts) {
                

                Ext.getCmp('eastform').expand();
                msglabel('訊息區:');

                T2LastRec = record;
                setFormT2a();

                setFormVisible(false, true);

                Ext.getCmp('eastform').expand();
                
            }
        }
    });

    var setFormVisible = function (t1Form, t2Form) {
        T1Form.setVisible(t1Form);
        T2Form.setVisible(t2Form);
    }

    // 按'新增'或'修改'時的動作
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();
        var f = T1Form.getForm();

        f.findField("PO_NO").setValue(T1LastRec.data["PO_NO"]);
        f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
        f.findField("PO_STATUS").setValue(T1LastRec.data["PO_STATUS"]);
        f.findField("ISBACK").setValue(T1LastRec.data["ISBACK"]);
        f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);
        f.findField("SMEMO").setValue(T1LastRec.data["SMEMO"]);
        f.findField("EMAIL").setValue(T1LastRec.data["EMAIL"]);

        f.findField('x').setValue(x);
        f.findField('MEMO').setReadOnly(false);
        f.findField('SMEMO').setReadOnly(false);
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
        var u = f.findField('MEMO');
        u.focus();
    }

    // 點選T1Grid一筆資料的動作
    function setFormT1a() {
        if (T1LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T1Form.getForm();
            f.findField("PO_NO").setValue(T1LastRec.data["PO_NO"]);
            f.findField("AGEN_NO").setValue(T1LastRec.data["AGEN_NO"]);
            f.findField("ISBACK").setValue(T1LastRec.data["ISBACK"]);
            f.findField("PO_STATUS").setValue(T1LastRec.data["PO_STATUS"]);
            f.findField("MEMO").setValue(T1LastRec.data["MEMO"]);
            f.findField("SMEMO").setValue(T1LastRec.data["SMEMO"]);
            f.findField("EMAIL").setValue(T1LastRec.data["EMAIL"]);

            T1Form.down('#cancel').setVisible(false);
            T1Form.down('#submit').setVisible(false);
        }

    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        layout: { type: 'table', columns: 1},
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
                fieldLabel: '訂單編號',
                name: 'PO_NO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '廠商',
                name: 'AGEN_NO',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: '訂單狀態',
                name: 'PO_STATUS',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'displayfield',
                fieldLabel: 'EMAIL',
                name: 'EMAIL',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            },
            {
                xtype: 'textareafield',
                fieldLabel: 'MAIL備註',
                name: 'MEMO',
                enforceMaxLength: true,
                maxLength: 4000,
                readOnly: true,
                height: 150,
                width:"100%"
            },
            {
                xtype: 'textareafield',
                fieldLabel: 'MAIL特殊備註',
                name: 'SMEMO',
                enforceMaxLength: true,
                maxLength: 4000,
                readOnly: true,
                height: 150,
                width: "100%"
            },
            {
                xtype: 'displayfield',
                fieldLabel: '回覆狀態',
                name: 'ISBACK',
                enforceMaxLength: true,
                readOnly: true,
                submitValue: true
            }
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
                    
                    T1Load();
                    switch (f2.findField("x").getValue()) {
                        case "I":
                            msglabel('訊息區:資料新增成功');
                            //Ext.Msg.alert('訊息', '新增成功');
                            break;
                        case "U":

                            msglabel('訊息區:資料更新成功');
                            //Ext.Msg.alert('訊息', '更新成功');
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

    function T1Cleanup() {
        viewport.down('#t1Grid').unmask();
        viewport.down('#t2Grid').unmask();
        Ext.getCmp('eastform').collapse();
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype === "displayfield" || fc.xtype === "textfield" || fc.xtype === "textareafield") {
                fc.setReadOnly(true);
            } else if (fc.xtype === "combo" || fc.xtype === "datefield") {
                fc.setReadOnly(true);
            }
        });
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        //setFormT1a();
    }

    // 按'新增'或'修改'時的動作
    function setFormT2(x, t) {
        
        viewport.down('#t1Grid').mask();
        viewport.down('#t2Grid').mask();
        viewport.down('#form').setTitle(t);
        viewport.down('#form').expand();

        var f = T2Form.getForm();
        if (x === "I") {
            isNew = true;
            var r = Ext.create('WEBAPP.model.ME_DOCD'); // /Scripts/app/model/MiUnitexch.js
            T2Form.loadRecord(r); // 建立空白model,在新增時載入T2Form以清空欄位內容
            u = f.findField("MMCODE"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();

            f.findField("DOCNO2").setValue(T1LastRec.data["DOCNO"]);
            f.findField("DOCNO").setValue(T1LastRec.data["DOCNO"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH"]);
            f.findField("WH_NO").setValue(T1LastRec.data["FRWH"]);
            f.findField("MMCODE").show();
            f.findField("MMCODE_DISPLAY").hide();
            Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').show();

            var promise = meDocdSeqPromise(f.findField("DOCNO2").getValue());
            promise.then(function (success) {
                var data = JSON.parse(success);
                if (data.success) {
                    var list = data.etts;
                    if (list.length > 0) {
                        var seq = list[0].SEQ;
                        f.findField("SEQ").setValue(seq);
                    }
                }
            });

            //T3Load();
        }
        else {

            f.findField("MMCODE").hide();
            f.findField("MMCODE_DISPLAY").show();

            u = f.findField('APVQTY');

            f.findField("DOCNO2").setValue(T2LastRec.data["DOCNO"]);
            f.findField("SEQ").setValue(T2LastRec.data["SEQ"]);
            f.findField("FRWH2").setValue(T1LastRec.data["FRWH_N"]);
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField("MMCODE_DISPLAY").setValue(T2LastRec.data["MMCODE"]);
            f.findField('APVQTY').setValue(T2LastRec.data["APVQTY"]);
            Ext.getCmp("mmcodeComboSet").getComponent('btnMmcode').hide();

            f.findField("WH_NO").setValue(T1LastRec.data["FRWH"]);
            f.findField("DOCNO").setValue(T2LastRec.data["DOCNO"]);
        }
        f.findField('x').setValue(x);
        //f.findField('MMCODE').setReadOnly(false);
        f.findField('APVQTY').setReadOnly(false);
        T2Form.down('#cancel').setVisible(true);
        T2Form.down('#submit').setVisible(true);
        u.focus();
    }
    function meDocdSeqPromise(docno) {
        var deferred = new Ext.Deferred();

        Ext.Ajax.request({
            url: GetMeDocdMaxSeq,
            method: reqVal_p,
            params: {
                DOCNO: docno
            },
            success: function (response) {
                deferred.resolve(response.responseText);
            },

            failure: function (response) {
                deferred.reject(response.status);
            }
        });

        return deferred.promise; //will return the underlying promise of deferred

    }

    // 點選T2Grid一筆資料的動作
    function setFormT2a() {
        if (T2LastRec != null) {
            //viewport.down('#form').setTitle(t + T1Name);
            viewport.down('#form').expand();
            var f = T2Form.getForm();
            
            f.findField("MMCODE").setValue(T2LastRec.data["MMCODE"]);
            f.findField('MMNAME_C').setValue(T2LastRec.data["MMNAME_C"]);
            f.findField('MMNAME_E').setValue(T2LastRec.data["MMNAME_E"]);
            f.findField('M_AGENLAB').setValue(T2LastRec.data["M_AGENLAB"]);
            f.findField('M_PURUN').setValue(T2LastRec.data["M_PURUN"]);
            f.findField('PO_PRICE').setValue(T2LastRec.data["PO_PRICE"]);
            f.findField('PO_QTY').setValue(T2LastRec.data["PO_QTY"]);
            f.findField('PO_AMT').setValue(T2LastRec.data["PO_AMT"]);
            f.findField('MEMO').setValue(T2LastRec.data["MEMO"]);
        }

    }

    // 顯示明細/新增/修改輸入欄
    var T2Form = Ext.widget({
        xtype: 'form',
        layout: 'form',
        frame: false,
        hidden: true,
        cls: 'T2b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        defaultType: 'textfield',
        items: [
            {
                xtype: 'container',
                items: [
                    {
                        xtype: 'displayfield',
                        fieldLabel: '院內碼',
                        name: 'MMCODE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '中文品名',
                        name: 'MMNAME_C',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '英文品名',
                        name: 'MMNAME_E',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '廠牌',
                        name: 'M_AGENLAB',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單位',
                        name: 'M_PURUN',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單價',
                        name: 'PO_PRICE',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '申請量',
                        name: 'PO_QTY',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '單筆價',
                        name: 'PO_AMT',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                    {
                        xtype: 'displayfield',
                        fieldLabel: '備註',
                        name: 'MEMO',
                        enforceMaxLength: true,
                        readOnly: true,
                        submitValue: true,
                        labelWidth: 90,
                        labelAlign: 'right'
                    },
                ]
            },
        ],
        
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
                width: 300,
                title: '瀏覽',
                border: false,
                collapsed: true,
                layout: {
                    type: 'fit',
                    padding: 5,
                    align: 'stretch'
                },
                items: [T1Form, T2Form]
            }
        ]
    });

    Ext.getCmp('btnUpdate').setDisabled(true);
    Ext.getCmp('btnSendMail').setDisabled(true);

});