Ext.Loader.setConfig({
    enabled: true,
    paths: {
        'WEBAPP': '/Scripts/app',
         'MODEL': '/Scripts/app/model'
    }
});

Ext.require(['WEBAPP.utils.Common']);

Ext.onReady(function () {
    // var T1Get = '/api/BE0008/All'; // 查詢(改為於store定義)
    var T1Set = ''; // 新增/修改/刪除
    var T1GetExcel = '../../../api/BE0008/Excel';
    var T1Name = "廠商基本檔維護";

    var T1Rec = 0;
    var T1LastRec = null;

    var col_labelWid = 90;
    var col_Wid = 90;

    Ext.override(Ext.grid.column.Column, { menuDisabled: true });

    Ext.getUrlParam = function (param) {
        var params = Ext.urlDecode(location.search.substring(1));
        return param ? params[param] : params;
    };
    var isH = Ext.getUrlParam('isH') == 'Y';

    var T1AgenBank14Combo = Ext.create('WEBAPP.form.QueryCombo', {
        fieldLabel: '銀行代碼',
        queryUrl: '/api/BE0008/GetAgenBank14Combo',
        extraFields: ['AGEN_BANK_14'],
        id: 'AGEN_BANK_14',
        name: 'AGEN_BANK_14',
        enforceMaxLength: true,
        maxLength: 15,
        readOnly: true,
        colspan: 2,
        width: col_Wid * 2,
        displayField: 'VALUE',
        listeners: {
            change: function (_this, newvalue, oldvalue) {
                var record = T1AgenBank14Combo.getStore().findRecord('VALUE', newvalue); // 查詢 id 為 2 的記錄
                if (record) {
                    var name = record.get('TEXT');
                    console.log('TEXT:', name);
                    var f = T1Form.getForm();
                    f.findField('BANK_NAME').setValue(name);
                } else {
                    console.log('Record not found');
                }
            },
        }
    });

    // 查詢欄位
    var mLabelWidth = 90;
    var mWidth = 220;
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
            xtype: 'panel',
            id: 'PanelP1',
            border: false,
            layout: 'hbox',
            items: [
                {
                    xtype: 'textfield',
                    fieldLabel: '廠商碼',
                    name: 'P0',
                    id: 'P0',
                    enforceMaxLength: true, // 限制可輸入最大長度
                    maxLength: 100, // 可輸入最大長度為100
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '廠商中文名稱',
                    name: 'P1',
                    id: 'P1',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '廠商英文名稱',
                    name: 'P2',
                    id: 'P2',
                    enforceMaxLength: true,
                    hidden:true,
                    maxLength: 100,
                    padding: '0 4 0 4'
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
                    fieldLabel: '廠商住址',
                    name: 'P3',
                    id: 'P3',
                    enforceMaxLength: true,
                    maxLength: 100,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'textfield',
                    fieldLabel: '統一編號',
                    name: 'P4',
                    id: 'P4',
                    enforceMaxLength: true,
                    maxLength: 9,
                    padding: '0 4 0 4'
                }, {
                    xtype: 'button',
                    text: '查詢',
                    handler: function () {
                        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
                        T1Load();
                        msglabel('訊息區:');
                    }
                }, {
                    xtype: 'button',
                    text: '清除',
                    handler: function () {
                        var f = this.up('form').getForm();
                        f.reset();
                        f.findField('P0').focus(); // 進入畫面時輸入游標預設在P0
                        msglabel('訊息區:');
                    }
                }
            ]
        }]
    });
    Ext.define('T1Model', {
        extend: 'Ext.data.Model',
        fields: [
            { name: 'AGEN_NO', type: 'string' },
        ]
    });
    var T1Store = Ext.create('Ext.data.Store', {
        model: 'T1Model',
        pageSize: 50, // 每頁顯示筆數
        remoteSort: true,
        sorters: [{ property: 'AGEN_NO', direction: 'ASC' }],
        proxy: {
            type: 'ajax',
            actionMethods: {
                read: 'POST' // by default GET
            },
            url: '/api/BE0008/All',
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
                    p0: T1Query.getForm().findField('P0').getValue(),
                    p1: T1Query.getForm().findField('P1').getValue(),
                    p2: T1Query.getForm().findField('P2').getValue(),
                    p3: T1Query.getForm().findField('P3').getValue(),
                    p4: T1Query.getForm().findField('P4').getValue()
                };
                Ext.apply(store.proxy.extraParams, np);
            }
        }
    });
    function T1Load() {
        T1Tool.moveFirst();
        T1LastRec = null;
    }

    // toolbar,包含換頁、新增/修改/刪除鈕
    var T1Tool = Ext.create('Ext.PagingToolbar', {
        store: T1Store,
        displayInfo: true,
        border: false,
        plain: true,
        buttons: [
            {
                text: '新增', handler: function () {
                    T1Set = '/api/BE0008/Create'; // BE0008Controller的Create
                    msglabel('訊息區:');
                    setFormT1('I', '新增');
                }
            },
            {
                itemId: 'edit', text: '修改', disabled: true, handler: function () {
                    T1Set = '/api/BE0008/Update';
                    msglabel('訊息區:');
                    setFormT1("U", '修改');
                }
            }, {
                itemId: 't1export', text: '匯出',
                handler: function () {
                    var p = new Array();
                    p.push({ name: 'p0', value: T1Query.getForm().findField('P0').getValue() });
                    p.push({ name: 'p1', value: T1Query.getForm().findField('P1').getValue() });
                    p.push({ name: 'p2', value: T1Query.getForm().findField('P2').getValue() });
                    p.push({ name: 'p3', value: T1Query.getForm().findField('P3').getValue() });
                    p.push({ name: 'p4', value: T1Query.getForm().findField('P4').getValue() });
                    p.push({ name: 'FN', value: '廠商基本檔維護.xlsx' });
                    PostForm(T1GetExcel, p);
                    msglabel('訊息區:匯出完成');
                }
            }
            //, {
            //    itemId: 'delete', text: '刪除', disabled: true,
            //    handler: function () {
            //        Ext.MessageBox.confirm('刪除', '是否確定刪除？', function (btn, text) {
            //            if (btn === 'yes') {
            //                T1Set = '/api/BE0008/Delete';
            //                T1Form.getForm().findField('x').setValue('D');
            //                T1Submit();
            //            }
            //        }
            //        );
            //    }
            //}
        ]
    });
    function setFormT1(x, t) {
        viewport.down('#t1Grid').mask();
        viewport.down('#form').setTitle(t + T1Name);
        //
        var f = T1Form.getForm();
        if (x === "I") {
            viewport.down('#form').expand();
            isNew = true;
            var r = Ext.create('WEBAPP.model.PhVender'); // /Scripts/app/model/PhVender.js
            T1Form.loadRecord(r); // 建立空白model,在新增時載入T1Form以清空欄位內容
            u = f.findField("AGEN_NO"); // 廠商碼在新增時才可填寫
            u.setReadOnly(false);
            u.clearInvalid();
            u.focus();
            //f.findField('REC_STATUS').setValue('A'); // 修改狀態碼預設為A
            //f.findField('MINODR_AMT').setValue('0');

            f.findField('AGEN_NAMEC').setReadOnly(false);
            f.findField('EASYNAME').setReadOnly(false);
            f.findField('UNI_NO').setReadOnly(false);
            f.findField('AGEN_BOSS').setReadOnly(false);
            f.findField('AGEN_FREETEL1').setReadOnly(false);
            f.findField('AGEN_FREETEL2').setReadOnly(false);
            f.findField('AGEN_ZIP').setReadOnly(false);
            f.findField('AGEN_ADD').setReadOnly(false);
            f.findField('AGEN_CONTACT').setReadOnly(false);
            f.findField('AGEN_CONCELL').setReadOnly(false);
            f.findField('AGEN_ZIP1').setReadOnly(false);
            f.findField('AGEN_ADD1').setReadOnly(false);
            f.findField('AGEN_CONTACT1').setReadOnly(false);
            f.findField('AGEN_CONCELL1').setReadOnly(false);
            f.findField('AGEN_ZIP2').setReadOnly(false);
            f.findField('AGEN_ADD2').setReadOnly(false);
            f.findField('AGEN_CONTACT2').setReadOnly(false);
            f.findField('AGEN_CONCELL2').setReadOnly(false);
            f.findField('AGEN_TEL').setReadOnly(false);
            f.findField('AGEN_TEL2').setReadOnly(false);
            f.findField('AGEN_TEL3').setReadOnly(false);
            f.findField('AGEN_TEL4').setReadOnly(false);
            f.findField('AGEN_FAX').setReadOnly(false);
            f.findField('AGEN_FAX2').setReadOnly(false);
            f.findField('AGEN_FAX3').setReadOnly(false);
            f.findField('AGEN_FAX4').setReadOnly(false);
            f.findField('EMAIL').setReadOnly(false);
            f.findField('EMAIL_1').setReadOnly(false);
            f.findField('AGEN_WEB1').setReadOnly(false);
            f.findField('AGEN_WEB2').setReadOnly(false);

        }
        else {
            viewport.down('#form').expand();
            if (isH) {
                f.findField('AGEN_BANK_14').setReadOnly(false);
                f.findField('AGEN_ACC').setReadOnly(false);
                f.findField('PROCFEE').setReadOnly(false);
                f.findField('POSTFEE').setReadOnly(false);
                f.findField('ACCZIP').setReadOnly(false);
                f.findField('ACCADDRESS').setReadOnly(false);
                f.findField('ACCCONTACT').setReadOnly(false);
                f.findField('ACCTEL').setReadOnly(false);
                f.findField('ACCEMAIL').setReadOnly(false);
                Ext.getCmp('setAccAsAbove').enable();
            } else {
                f.findField('AGEN_NAMEC').setReadOnly(false);
                f.findField('EASYNAME').setReadOnly(false);
                f.findField('UNI_NO').setReadOnly(false);
                f.findField('AGEN_BOSS').setReadOnly(false);
                f.findField('AGEN_FREETEL1').setReadOnly(false);
                f.findField('AGEN_FREETEL2').setReadOnly(false);
                f.findField('AGEN_ZIP').setReadOnly(false);
                f.findField('AGEN_ADD').setReadOnly(false);
                f.findField('AGEN_CONTACT').setReadOnly(false);
                f.findField('AGEN_CONCELL').setReadOnly(false);
                f.findField('AGEN_ZIP1').setReadOnly(false);
                f.findField('AGEN_ADD1').setReadOnly(false);
                f.findField('AGEN_CONTACT1').setReadOnly(false);
                f.findField('AGEN_CONCELL1').setReadOnly(false);
                f.findField('AGEN_ZIP2').setReadOnly(false);
                f.findField('AGEN_ADD2').setReadOnly(false);
                f.findField('AGEN_CONTACT2').setReadOnly(false);
                f.findField('AGEN_CONCELL2').setReadOnly(false);
                f.findField('AGEN_TEL').setReadOnly(false);
                f.findField('AGEN_TEL2').setReadOnly(false);
                f.findField('AGEN_TEL3').setReadOnly(false);
                f.findField('AGEN_TEL4').setReadOnly(false);
                f.findField('AGEN_FAX').setReadOnly(false);
                f.findField('AGEN_FAX2').setReadOnly(false);
                f.findField('AGEN_FAX3').setReadOnly(false);
                f.findField('AGEN_FAX4').setReadOnly(false);
                f.findField('EMAIL').setReadOnly(false);
                f.findField('EMAIL_1').setReadOnly(false);
                f.findField('AGEN_WEB1').setReadOnly(false);
                f.findField('AGEN_WEB2').setReadOnly(false);
            }
        }
        f.findField('x').setValue(x);
        
        T1Form.down('#cancel').setVisible(true);
        T1Form.down('#submit').setVisible(true);
     
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
            layout: 'fit',
            items: [T1Query]
        }, {
            dock: 'top',
            xtype: 'toolbar',
            items: [T1Tool]
        }],
        columns: [{
            xtype: 'rownumberer'
        }, {
            text: "廠商碼",
            dataIndex: 'AGEN_NO',
            width: 80
        }, {
            text: "名稱",
            dataIndex: 'AGEN_NAMEC',
            width: 130
        }, {
            text: "簡稱",
            dataIndex: 'EASYNAME',
            width: 80
        }, {
            text: "統一編號",
            dataIndex: 'UNI_NO',
            width: 80
        }, {
            text: "負責人",
            dataIndex: 'AGEN_BOSS',
            width: 80
        }, {
            text: "廠商住址",
            dataIndex: 'AGEN_ADD',
            width: 150
        }, {
            text: "傳真號碼",
            dataIndex: 'AGEN_FAX',
            width: 100
        }, {
            text: "電話號碼",
            dataIndex: 'AGEN_TEL',
            width: 100
        },
        {
            text: "EMAIL",
            dataIndex: 'EMAIL',
            width: 150
        }, {
            text: "EMAIL_1",
            dataIndex: 'EMAIL_1',
            width: 150
        }, {
            text: "銀行代碼",
            dataIndex: 'AGEN_BANK_14',
            width: 70
        },{
            text: "銀行名稱",
            dataIndex: 'BANK_NAME',
            width: 100
        }, {
            text: "銀行帳號",
            dataIndex: 'AGEN_ACC',
            width: 120
        },  {
            header: "",
            flex: 1
        }],
        listeners: {
            selectionchange: function (model, records) {
                T1Rec = records.length;
                T1LastRec = records[0];
                if (T1LastRec) {
                    Ext.ComponentQuery.query('panel[itemId=form]')[0].expand();
                    setFormT1a();
                }   
            }
        }
    });
    function setFormT1a() {
        T1Grid.down('#edit').setDisabled(T1Rec === 0);
        // T1Grid.down('#delete').setDisabled(T1Rec === 0); // 若有刪除鈕,可在此控制是否可以按
        if (T1LastRec) {
            isNew = false;
            T1Form.loadRecord(T1LastRec);
            
            var f = T1Form.getForm();
            f.findField('x').setValue('U');
            var u = f.findField('AGEN_NO');
            u.setReadOnly(true);
            u.setFieldStyle('border: 0px');
            //if (T1LastRec.data['REC_STATUS'] == 'X')
            //    T1Grid.down('#edit').setDisabled(true); // 停用的資料就不允許修改
        }
        else {
            T1Form.getForm().reset();
        }
    }

    // 顯示明細/新增/修改輸入欄
    var T1Form = Ext.widget({
        xtype: 'form',
        //layout: 'form',
        layout: {
            type: 'table',
            columns: 10,
            tdAttrs: { width: col_Wid }
        },
        frame: false,
        cls: 'T1b',
        title: '',
        autoScroll: true,
        bodyPadding: '5 5 0',
        fieldDefaults: {
            labelAlign: 'right',
            msgTarget: 'side',
            labelWidth: col_labelWid,
            width: col_Wid,
        },
        defaultType: 'textfield',
        items: [
            {
                fieldLabel: 'Update',
                name: 'x',
                xtype: 'hidden'
            }, {
                fieldLabel: '廠商碼',
                name: 'AGEN_NO',
                enforceMaxLength: true,
                maxLength: 10,
                regexText: '只能輸入英文字母與數字',
                regex: /^[\w]{0,10}$/, // 用正規表示式限制可輸入內容
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                colspan: 2,
                width: col_Wid * 2,
                listeners: {
                    blur: function () {

                        if (T1Form.getForm().findField('x').getValue() == 'I') {
                            Ext.Ajax.request({
                                url: '/api/BE0008/CheckExists',
                                method: reqVal_p,
                                params: {
                                    AGEN_NO: T1Form.getForm().findField('AGEN_NO').getValue()
                                },
                                success: function (response) {
                                    var data = Ext.decode(response.responseText);
                                    if (data.success) {

                                    } else {
                                        Ext.MessageBox.alert('提示', data.msg);
                                    }
                                },
                                failure: function (response) {
                                    Ext.MessageBox.alert('錯誤', '發生例外錯誤');
                                }
                            });
                        }
                    }
                }
            }, {
                fieldLabel: '名稱',
                name: 'AGEN_NAMEC',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                allowBlank: false, // 欄位為必填
                fieldCls: 'required',
                colspan: 4,
                width: col_Wid * 4,
            }, {
                fieldLabel: '簡稱',
                name: 'EASYNAME',
                enforceMaxLength: true,
                maxLength: 20,
                colspan: 4,
                width: col_Wid * 2,
            }, {
                fieldLabel: '統一編號',
                name: 'UNI_NO',
                enforceMaxLength: true,
                maxLength: 8,
                maskRe: /[0-9]/,
                regex: /^([0-9]{8})$/,
                regexText: '格式需為八位數字',
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            }, {
                fieldLabel: '負責人',
                name: 'AGEN_BOSS',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '免費電話一',
                name: 'AGEN_FREETEL1',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '免費電話二',
                name: 'AGEN_FREETEL2',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 2,
            },

            {
                xtype: 'displayfield',
                fieldLabel: '郵遞區號',
                labelSeparator: '',
                width: col_Wid,
                labelAlign: 'left',
            },
            {
                xtype: 'displayfield',
                fieldLabel: '地址',
                labelSeparator: '',
                colspan: 3,
                width: col_Wid * 5,
                labelAlign: 'left',
            },
            {
                xtype: 'displayfield',
                fieldLabel: '聯絡人',
                labelSeparator: '',
                colspan: 1,
                width: col_Wid * 1,
                labelAlign: 'left',
            },
            {
                xtype: 'displayfield',
                fieldLabel: '行動電話',
                labelSeparator: '', colspan: 5,
                width: col_Wid * 1,
                labelAlign: 'left',
            },
            {
                fieldLabel: '',
                name: 'AGEN_ZIP',
                labelSeparator: '',
                width: col_Wid,
                readOnly: true,
            },
            {
                fieldLabel: '',
                name: 'AGEN_ADD',
                labelSeparator: '',
                colspan: 3,
                width: col_Wid * 5,
                readOnly: true,
            },
            {
                fieldLabel: '',
                name: 'AGEN_CONTACT',
                labelSeparator: '', colspan: 1,
                width: col_Wid * 1,
                readOnly: true,
                labelWidth: 0
            },
            {
                fieldLabel: '',
                name: 'AGEN_CONCELL',
                labelSeparator: '', colspan: 5,
                width: col_Wid * 1,
                readOnly: true,
                labelWidth: 0
            },
            {
                fieldLabel: '',
                name: 'AGEN_ZIP1',
                labelSeparator: '',
                width: col_Wid,
                readOnly: true,
            },
            {
                fieldLabel: '',
                name: 'AGEN_ADD1',
                labelSeparator: '',
                colspan: 3,
                width: col_Wid * 5,
                readOnly: true,
                labelWidth: 0
            },
            {
                fieldLabel: '',
                name: 'AGEN_CONTACT1',
                labelSeparator: '', colspan: 1,
                width: col_Wid * 1,
                readOnly: true,
                labelWidth: 0
            },
            {
                fieldLabel: '',
                name: 'AGEN_CONCELL1',
                labelSeparator: '', colspan: 5,
                width: col_Wid * 1,
                readOnly: true,
                labelWidth: 0
            },
            {
                fieldLabel: '',
                name: 'AGEN_ZIP2',
                labelSeparator: '',
                width: col_Wid,
                readOnly: true,
            },
            {
                fieldLabel: '',
                name: 'AGEN_ADD2',
                labelSeparator: '',
                colspan: 3,
                width: col_Wid * 5,
                readOnly: true,
            },
            {
                fieldLabel: '',
                name: 'AGEN_CONTACT2',
                labelSeparator: '',
                colspan: 1,
                width: col_Wid * 1,
                readOnly: true,
            },
            {
                fieldLabel: '',
                name: 'AGEN_CONCELL2',
                labelSeparator: '',
                colspan: 5,
                width: col_Wid * 1,
                readOnly: true,
            },
            {
                fieldLabel: '電話一',
                name: 'AGEN_TEL',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '電話二',
                name: 'AGEN_TEL2',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '電話三',
                name: 'AGEN_TEL3',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '電話四',
                name: 'AGEN_TEL4',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '傳真一',
                name: 'AGEN_FAX',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '傳真二',
                name: 'AGEN_FAX2',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '傳真三',
                name: 'AGEN_FAX3',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '傳真四',
                name: 'AGEN_FAX4',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 2,
            },
            {
                fieldLabel: 'EMAIL 一',
                name: 'EMAIL',
                enforceMaxLength: true,
                maxLength: 60,
                regex: /^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/,
                regexText: '需為email格式,例如abc@mail.com',
                readOnly: true,
                colspan: 4,
                width: col_Wid * 4,
            },
            {
                fieldLabel: 'EMAIL 二',
                name: 'EMAIL_1',
                enforceMaxLength: true,
                maxLength: 60,
                regex: /^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/,
                regexText: '需為email格式,例如abc@mail.com',
                readOnly: true,
                colspan: 6,
                width: col_Wid * 4,
            },
            {
                fieldLabel: '公司網站一',
                name: 'AGEN_WEB1',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 4,
            },
            {
                fieldLabel: '公司網站二',
                name: 'AGEN_WEB2',
                enforceMaxLength: true,
                maxLength: 60,
                readOnly: true,
                colspan: 6,
                width: col_Wid * 4,
            },

            {
                xtype: 'displayfield',
                fieldLabel: '<span style="color:blue">《廠商銀行帳戶設定》</span>',
                labelSeparator: '',
                colspan: 10,
                width: col_Wid * 10,
                labelWidth: col_Wid * 2
            },
            //{
            //    fieldLabel: '銀行代碼',
            //    name: 'AGEN_BANK_14',
            //    enforceMaxLength: true,
            //    maxLength: 15,
            //    readOnly: true,
            //    colspan: 2,
            //    width: col_Wid * 2,
            //},
            T1AgenBank14Combo,
            {
                xtype:'displayfield',
                fieldLabel: '銀行名稱',
                name: 'BANK_NAME',
                enforceMaxLength: true,
                maxLength: 15,
                readOnly: true,
                colspan: 8,
                width: col_Wid * 4,
            },
            {
                fieldLabel: '銀行帳號',
                name: 'AGEN_ACC',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 4,
            },
            {
                fieldLabel: '匯款手續費',
                name: 'PROCFEE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '郵寄費用',
                name: 'POSTFEE',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '郵遞區號',
                name: 'ACCZIP',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '地址',
                name: 'ACCADDRESS',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 4,
            },
            {
                fieldLabel: '聯絡人',
                name: 'ACCCONTACT',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                colspan: 4,
                width: col_Wid * 2,
            },
            {
                fieldLabel: '電話',
                name: 'ACCTEL',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                colspan: 2,
                width: col_Wid * 2,
            },
            {
                fieldLabel: 'EMAIL',
                name: 'ACCEMAIL',
                enforceMaxLength: true,
                maxLength: 60,
                regex: /^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$/,
                regexText: '需為email格式,例如abc@mail.com',
                readOnly: true,
                colspan: 4,
                width: col_Wid * 4,
            }
            , {
                xtype: 'button',
                text: '設為上面第一筆資料',
                enforceMaxLength: true,
                maxLength: 100,
                readOnly: true,
                disabled: true,
                id: 'setAccAsAbove',
                handler: function () {
                    var f = T1Form.getForm();
                    f.findField('ACCZIP').setValue(f.findField('AGEN_ZIP').getValue());
                    f.findField('ACCADDRESS').setValue(f.findField('AGEN_ADD').getValue());
                    f.findField('ACCCONTACT').setValue(f.findField('AGEN_CONTACT').getValue());
                    f.findField('ACCTEL').setValue(f.findField('AGEN_TEL').getValue());
                    f.findField('ACCEMAIL').setValue(f.findField('EMAIL').getValue());
                }
            },
            ],
        buttons: [{
            itemId: 'submit', text: '儲存', hidden: true,
            handler: function () {
                if (this.up('form').getForm().isValid()) { // 檢查T1Form填寫資料是否符合規則(必填欄位都有填、輸入內容有符合正規表示式等)
                    if (this.up('form').getForm().findField('AGEN_NAMEC').getValue() == '')
                        Ext.Msg.alert('提醒', '請輸入廠商名稱');
                    else {
                        var confirmSubmit = viewport.down('#form').title.substring(0, 2);
                        Ext.MessageBox.confirm(confirmSubmit, '是否確定' + confirmSubmit + '?', function (btn, text) {
                            if (btn === 'yes') {
                                T1Submit();
                            }
                        }
                        );
                    }
                }
                else {
                    Ext.Msg.alert('提醒', '輸入資料格式有誤');
                    msglabel('訊息區:輸入資料格式有誤');
                }
            }
        }, {
            itemId: 'cancel', text: '取消', hidden: true, handler: T1Cleanup
        }]
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
                            // 新增後,將key代入查詢條件,只顯示剛新增的資料
                            var v = action.result.etts[0];
                            T1Query.getForm().findField('P0').setValue(v.AGEN_NO);
                            T1Load();
                            msglabel('訊息區:資料新增成功');
                            break;
                        case "U":
                            var v = action.result.etts[0];
                            r.set(v);
                            r.commit();
                            msglabel('訊息區:資料修改成功');
                            break;
                        //case "D":
                        //    T1Store.remove(r); // 若刪除後資料需從查詢結果移除可用remove
                        //    r.commit();
                        //    break;
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
        var f = T1Form.getForm();
        f.reset();
        f.getFields().each(function (fc) {
            if (fc.xtype == "displayfield" || fc.xtype == "textfield" || fc.xtype == "numberfield") {
                fc.setReadOnly(true);
            } else if (fc.xtype == "combo" || fc.xtype == "datefield") {
                fc.readOnly = true;
            }
        });
        Ext.getCmp('setAccAsAbove').disable();
        T1Form.down('#cancel').hide();
        T1Form.down('#submit').hide();
        viewport.down('#form').setTitle('瀏覽');
        Ext.ComponentQuery.query('panel[itemId=form]')[0].collapse();
        setFormT1a();
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
            items: [T1Grid]
        },
        {
            itemId: 'form',
            region: 'east',
            collapsible: true,
            floatable: true,
            width: '70%',
            title: '瀏覽',
            border: false,
            collapsed: true,
            layout: {
                type: 'fit',
                padding: 5,
                align: 'stretch'
            },
            items: [T1Form]
        }
        ]
    });

    //T1Load(); // 進入畫面時自動載入一次資料
    T1Query.getForm().findField('P0').focus();
});
